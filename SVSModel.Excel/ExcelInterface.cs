// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using SVSModel.Configuration;
using ExcelDna.Integration;
using System.Linq;
using SVSModel.Models;
using static SVSModel.Configuration.Constants;
using static SVSModel.Configuration.InputCategories;
using System.Runtime.InteropServices.ComTypes;
using SVSModel;
using System.Net;
using SVSModel.Simulation;

namespace SVSModel.Excel
{
    public interface IMyFunctions
    {
        object[,] GetDailyNBalance(object[,] config, object[,] testResults, object[,] nApplied);

        object[,] GetDailyNBalanceSummary(object[,] config, object[,] testResults, object[,] nApplied);

        object[,] GetDailyCropData(double[] Tt, object[,] Config);

        object[,] GetCropCoefficients();

        object[,] GetSoilTestResult(object testDate, double testValue, string depthOfSample,
                                               string typeOfTest, string moistureOfTest,
                                               string soilCategory, string soilTexture);
    }

    public static class MyFunctions //: IMyFunctions
    {
        /// <summary>
        /// Takes soil test input values and returns a value in kg N/ha
        /// </summary>
        /// <param name="testDate">Date of test</param>
        /// <param name="testValue">value of test in mgN/g soil</param>
        /// <param name="depthOfSample">0-15, 30, 60 or 90cm</param>
        /// <param name="typeOfTest">Lab or Quicktest</param>
        /// <param name="moistureOfTest">Wet, Moist, dry</param>
        /// <param name="categoryOfSoil">Volcanic or sedementary</param>
        /// <param name="textureOfSoil">a valid texture class</param>
        /// <returns>Soil nitrogen in kg/ha</returns>
        public static object[,] GetSoilTestResult(object testDate, double testValue, 
                                                                     string depthOfSample,
                                                                     string typeOfTest, string moistureOfTest,
                                                                     string categoryOfSoil, string textureOfSoil)

        {
            DateTime _testDate = Functions.Date(testDate);
            SoilTestConfig test = new SoilTestConfig(_testDate, testValue, depthOfSample,
                                                       typeOfTest, moistureOfTest,
                                                       categoryOfSoil, textureOfSoil);
            object[,] st = new object[1, 2];
            st[0, 0] = test.Result.Keys.First();
            st[0, 1] = test.Result.Values.First();
            return st;
        }



        /// <summary>
        /// Function that takes input data in 2D array format and calculates a N balance for a 3 crops rotation and returns N balance variables in 2D array format
        /// </summary>
        /// <param name="met">2D Array with dates in first column and daily meterological data over the duration of the rotation in the second column</param>
        /// <param name="config">2D array with parameter names and values for crop field configuration parameters</param>
        /// <param name="testResults">2D array with dates in teh first column and soil N test results in the second</param>
        /// <returns>Dictionary with parameter names as keys and parameter values as values</returns>
        [ExcelFunction(Description = "Returns full N balance results")]
        public static object[,] GetDailyNBalance(object[,] config, object[,] testResults, object[,] nApplied)
        {
            List<string> configErrors = Functions.ValidateConfig(config);

                if (configErrors.Count == 0)
            {
                var _config = new Config(Functions.dictMaker(config));

                var startDate = _config.Prior.EstablishDate.AddDays(-1);
                var endDate = _config.Following.HarvestDate.AddDays(2);
                var weatherStation = _config.Field.WeatherStation;
                MetDataDictionaries metData = ModelInterface.BuildMetDataDictionaries(startDate, endDate, weatherStation, false);

                Dictionary<DateTime, double> _testResults = Functions.dictMaker(testResults, "Value");
                Dictionary<DateTime, double> _nApplied = Functions.dictMaker(nApplied, "Amount");

                return Simulation.Simulation.SimulateField(metData.MeanT, metData.Rain, metData.MeanPET, _testResults, _nApplied, _config, Constants.InitialN);
            }
            else
            {
                object[,] listOfComplaints = new object[configErrors.Count, 3];
                int c = 0;
                foreach (string e in configErrors)
                {
                    listOfComplaints[c, 2] = e;
                    c++;
                }
                return listOfComplaints;
            }
        }

        /// <summary>
        /// Function that takes input data in 2D array format and calculates a N balance for a 3 crops rotation and returns N balance variables in 2D array format
        /// </summary>
        /// <param name="met">2D Array with dates in first column and daily meterological data over the duration of the rotation in the second column</param>
        /// <param name="config">2D array with parameter names and values for crop field configuration parameters</param>
        /// <param name="testResults">2D array with dates in teh first column and soil N test results in the second</param>
        /// <returns>Dictionary with parameter names as keys and parameter values as values</returns>
        [ExcelFunction(Description = "Returns full N balance results")]
        public static object[,] GetDailyNBalanceSummary(object[,] config, object[,] testResults, object[,] nApplied)
        {
            List<string> configErrors = Functions.ValidateConfig(config);

            if (configErrors.Count == 0)
            {
                var _config = new Config(Functions.dictMaker(config));

                var startDate = _config.Prior.EstablishDate.AddDays(-1);
                var endDate = _config.Following.HarvestDate.AddDays(2);
                var weatherStation = _config.Field.WeatherStation;
                MetDataDictionaries metData = ModelInterface.BuildMetDataDictionaries(startDate, endDate, weatherStation, false);

                Dictionary<DateTime, double> _testResults = Functions.dictMaker(testResults, "Value");
                Dictionary<DateTime, double> _nApplied = Functions.dictMaker(nApplied, "Amount");

                var rawResult = Simulation.Simulation.SimulateField(metData.MeanT, metData.Rain, metData.MeanPET, _testResults, _nApplied, _config, Constants.InitialN);

                NBalanceSummary nBalSum = new NBalanceSummary(Simulation.Simulation.thisSim.CurrentNBalanceSummary);

                object[,] outputs = new object[3, 9];

                outputs[0, 0] = "Mineral"; Functions.packRows(0, nBalSum.Mineral, ref outputs);
                outputs[0, 1] = "UptakeN"; Functions.packRows(1, new Dictionary<string, int>() { { "In", 0 },{ "Out", 0 } }, ref outputs);
                outputs[0, 2] = "Residue"; Functions.packRows(2, nBalSum.Residues, ref outputs);
                outputs[0, 3] = "Organic"; Functions.packRows(3, nBalSum.SoilOrganic, ref outputs);
                outputs[0, 4] = "Fertiliser"; Functions.packRows(4, nBalSum.Fertiliser, ref outputs);
                outputs[0, 5] = "Other Crop Parts"; Functions.packRows(5, nBalSum.OtherCropParts, ref outputs);
                outputs[0, 6] = "Crop Product"; Functions.packRows(6, nBalSum.CropProduct, ref outputs);
                outputs[0, 7] = "Uncharacterised"; Functions.packRows(7, nBalSum.UnCharacterised, ref outputs);
                outputs[0, 8] = "Total"; Functions.packRows(8, nBalSum.Total, ref outputs);

                return outputs;
            }

            else
            {
                object[,] listOfComplaints = new object[configErrors.Count, 3];
                int c = 0;
                foreach (string e in configErrors)
                {
                    listOfComplaints[c, 2] = e;
                    c++;
                }
                return listOfComplaints;
            }
        }

        [ExcelFunction(Description = "Gets crop coefficient table")]
        public static object[,] GetCropCoefficients()
        {
            //Trace.WriteLine("exxcelentinterface");
            return Functions.packDataFrame(Crop.LoadCropCoefficients());
        }

        
    }
}
