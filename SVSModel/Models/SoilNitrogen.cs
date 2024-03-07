using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using SVSModel.Configuration;
using SVSModel.Simulation;

namespace SVSModel.Models
{
    public class SoilNitrogen
    {
        /// <summary>
        /// Calculates soil mineral nitrogen from an assumed initial value and modeled crop uptake and mineralisation from residues and soil organic matter
        /// </summary>
        /// <param name="uptake">series of daily N uptake values over the duration of the rotatoin</param>
        /// <param name="residue">series of mineral N released daily to the soil from residue mineralisation</param>
        /// <param name="som">series of mineral N released daily to the soil from organic matter</param>
        /// <returns>date indexed series of estimated soil mineral N content</returns>
        public static Dictionary<DateTime, double> InitialBalance(
            Dictionary<DateTime, double> uptake,
            Dictionary<DateTime, double> residue,
            Dictionary<DateTime, double> som,
            Config config)
        {
            DateTime[] simDates = uptake.Keys.ToArray();
            Dictionary<DateTime, double> soilN = Functions.dictMaker(simDates, new double[simDates.Length]);
            foreach (DateTime d in simDates)
            {
                if (d == simDates[0])
                {
                    soilN[simDates[0]] = config.Field.InitialN;
                }
                else
                {
                    soilN[d] = soilN[d.AddDays(-1)];
                }
                soilN[d] += residue[d];
                soilN[d] += som[d];
                double actualUptake = uptake[d]; //Math.Min(uptake[d], minN[d]);
                soilN[d] -= actualUptake;
            }
            return soilN;
        }

        /// <summary>
        /// Calculates soil mineral nitrogen from an assumed initial value and modeled crop uptake and mineralisation from residues and soil organic matter
        /// </summary>
        /// <param name="uptake">series of daily N uptake values over the duration of the rotatoin</param>
        /// <param name="residue">series of mineral N released daily to the soil from residue mineralisation</param>
        /// <param name="som">series of mineral N released daily to the soil from organic matter</param>
        /// <returns>date indexed series of estimated soil mineral N content</returns>
        public static void UpdateBalance(DateTime updateDate, double StartDayN, ref SimulationType thisSim)
        {
            DateTime[] updateDates = Functions.DateSeries(updateDate, thisSim.config.Following.HarvestDate);
            foreach (DateTime d in updateDates)
            {
                CheckNBalance todayCheck = new CheckNBalance();
                if (d == updateDate)
                {
                    thisSim.SoilN[d] = StartDayN;
                }
                else
                {
                    thisSim.SoilN[d] = thisSim.SoilN[d.AddDays(-1)];
                    todayCheck = new CheckNBalance(thisSim.SoilN[d.AddDays(-1)], thisSim.CropN[d.AddDays(-1)], 
                                                    thisSim.FertiliserN[d], thisSim.TransPlantN[d], thisSim.ExportN[d]);
                }

                thisSim.SoilN[d] += thisSim.NResidues[d];
                todayCheck.dResidueN = thisSim.NResidues[d];
                thisSim.SoilN[d] += thisSim.NSoilOM[d];
                todayCheck.dSOMN += thisSim.NSoilOM[d];
                double actualUptake = thisSim.NUptake[d];//Math.Min(thisSim.NUptake[d], thisSim.SoilN[d]*.1);
                //double Nshortage = thisSim.NUptake[d] - actualUptake;
                //if (Nshortage < 0)
                //    Crop.ConstrainNUptake(ref thisSim, Nshortage,d);
                todayCheck.CropN = thisSim.CropN[d];
                thisSim.SoilN[d] -= actualUptake;
                thisSim.LostN[d] = Losses.DailyLoss(d, thisSim);
                todayCheck.LostN = thisSim.LostN[d];
                thisSim.SoilN[d] -= thisSim.LostN[d];
                todayCheck.FinalMinearlN = thisSim.SoilN[d];
                if (d != updateDate)
                    todayCheck.DoCheck();

            }

        }

        /// <summary>
        /// Takes soil mineral N test values and adjustes to predicted N balance to correspond with these values on their specific dates
        /// </summary>
        /// <param name="testResults">date indexed series of test results</param>
        /// <param name="soilN">date indexed series of soil mineral N estimates to be corrected with measurements.  Passed in as ref so 
        /// the corrections are applied to the property passed in</param>
        public static void TestCorrection(Dictionary<DateTime, double> testResults, ref SimulationType thisSim)
        {
            foreach (DateTime d in testResults.Keys)
            {
                SoilNitrogen.UpdateBalance(d, testResults[d], ref thisSim);
            }
        }
    }
    public class CheckNBalance
    {

        /// IN
        public double INs
        {
            get
            {
                return InitialN + TransplantN + InitialCropN + dResidueN + dSOMN + dFertN;
            }
        }
        public double InitialN { get; set; }
        public double TransplantN { get; set; }
        public double InitialCropN { get; set; }
        public double dResidueN { get; set; }
        public double dSOMN { get; set; }
        public double dFertN { get; set; }


        /// Out
        public double OUTs
        {
            get
            {
                return FinalMinearlN + CropN + LostN + ExportN;
            }
        }
        public double FinalMinearlN { get; set; }
        public double CropN { get; set; }
        public double LostN { get; set; }
        public double ExportN { get; set; }

        public void DoCheck()
        {
            if (Math.Abs(INs - OUTs) > 0.000001)
                throw new Exception("Mass balance violated");
        }

        public CheckNBalance() { }
        public CheckNBalance(double initialN, double cropN, double fertN, double transN, double exportN)
        {
            InitialN = initialN;
            InitialCropN = cropN;
            dFertN = fertN;
            TransplantN = transN;
            ExportN = exportN;
        }
    }

}
