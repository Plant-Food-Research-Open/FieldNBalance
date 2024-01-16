using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CsvHelper;
using SVSModel.Configuration;
using SVSModel.Models;

namespace SVSModel
{
    public interface IModelInterface
    {
        /// <summary>
        /// User friendly interface for calling SimulateField
        /// </summary>
        /// <param name="weatherStation">A string representing the closest weather station 'gore' | 'hastings' | 'levin' | 'lincoln' | 'pukekohe'</param>
        /// <param name="testResults">A dictionary of nitrogen test results</param>
        /// <param name="nApplied">A dictionary of nitrogen applications</param>
        /// <param name="config">Model config object, all parameters are required</param>
        /// <returns>A list of <see cref="DailyNBalance"/> objects</returns>
        List<DailyNBalance> GetDailyNBalance(string weatherStation, Dictionary<DateTime, double> testResults, Dictionary<DateTime, double> nApplied, Config config);
        /// <summary>
        /// Gets the crop data from the data file
        /// </summary>
        /// <returns>List of <see cref="CropCoefficient"/>s directly from the data file</returns>
        IEnumerable<CropCoefficient> GetCropCoefficients();

        object[,] GetDailyCropData(double[] Tt, object[,] Config);
    }

    public class ModelInterface : IModelInterface
    {
        public List<DailyNBalance> GetDailyNBalance(string weatherStation, Dictionary<DateTime, double> testResults, Dictionary<DateTime, double> nApplied, Config config)
        {
            var startDate = config.Prior.EstablishDate.AddDays(-1);
            var endDate = config.Following.HarvestDate.AddDays(2);
            var metData = BuildMetDataDictionaries(startDate, endDate, weatherStation);

            var rawResult = Simulation.SimulateField(metData.MeanT, metData.Rain, metData.MeanPET, testResults, nApplied, config);

            var result = new List<DailyNBalance>();

            // Convert from the 2d object array that SimulateField returns into something user friendly
            for (var r = 1; r < rawResult.GetLength(0); r++)
            {
                var row = Enumerable.Range(0, rawResult.GetLength(1))
                    .Select(x => rawResult[r, x])
                    .ToList();

                var values = row.Skip(1).OfType<double>().ToArray();

                var data = new DailyNBalance
                {
                    Date = (DateTime)row[0],
                    SoilMineralN = values[0],
                    UptakeN = values[1],
                    ResidueN = values[2],
                    SoilOMN = values[3],
                    FertiliserN = values[4],
                    CropN = values[5],
                    ProductN = values[6],
                    LostN = values[7],
                    RSWC = values[8],
                    Drainage = values[9],
                    Irrigation = values[10],
                    GreenCover = values[11],
                };

                result.Add(data);
            }

            return result;
        }

        public IEnumerable<CropCoefficient> GetCropCoefficients()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream("SVSModel.Data.CropCoefficientTableFull.csv");
            if (stream == null) return Enumerable.Empty<CropCoefficient>();

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CropCoefficientMap>();

                var cropData = csv.GetRecords<CropCoefficient>();
                return cropData.ToList();
            }
        }

        private static IEnumerable<WeatherStationData> GetMetData(string weatherStation)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream($"SVSModel.Data.Met.{weatherStation}.csv");
            if (stream == null) return Enumerable.Empty<WeatherStationData>();

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var data = csv.GetRecords<WeatherStationData>();
                return data.ToList();
            }
        }

        public static MetDataDictionaries BuildMetDataDictionaries(DateTime startDate, DateTime endDate, string weatherStation)
        {
            var metData = GetMetData(weatherStation).ToList();

            var meanT = new Dictionary<DateTime, double>();
            var rain = new Dictionary<DateTime, double>();
            var meanPET = new Dictionary<DateTime, double>();

            var currDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            while (currDate < endDate)
            {
                var doy = currDate.DayOfYear;
                var values = metData.FirstOrDefault(m => m.DOY == doy);

                meanT.Add(currDate, values?.MeanT ?? 0);
                rain.Add(currDate, values?.Rain ?? 0);
                meanPET.Add(currDate, values?.MeanPET ?? 0);

                currDate = currDate.AddDays(1);
            }
            
            return new MetDataDictionaries { MeanT = meanT, Rain = rain, MeanPET = meanPET };
        }

        /// <summary>
        /// Takes daily mean temperature 2D array format with date in the first column, calculates variables for a single crop and returns them in a 2D array)
        /// </summary>
        /// <param name="Tt">Array of daily thermal time over the duration of the crop</param>
        /// <param name="Config">2D aray with parameter names and values for crop configuration parameters</param>
        /// <returns>Dictionary with parameter names as keys and parameter values as values</returns>
        public object[,] GetDailyCropData(double[] Tt, object[,] Config)
        {
            Dictionary<string, object> c = Functions.dictMaker(Config);
            CropConfig config = new CropConfig(c, "Current");
            DateTime[] cropDates = Functions.DateSeries(config.EstablishDate, config.HarvestDate);
            Dictionary<DateTime, double> tt = Functions.dictMaker(cropDates, Tt);
            Dictionary<DateTime, double> AccTt = Functions.AccumulateTt(cropDates, tt);
            Trace.WriteLine("I have made it ");
            return Crop.Grow(AccTt, config);
        }
    }
}
