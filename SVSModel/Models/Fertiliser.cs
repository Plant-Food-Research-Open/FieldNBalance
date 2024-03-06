using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using SVSModel.Configuration;
using SVSModel.Simulation;

namespace SVSModel.Models
{
    public class Fertiliser
    {
        /// <summary>
        /// Finds the last date that fertiliser was applied or a test was entered and returns that
        /// </summary>
        /// <param name="fert">The fertiliser already applied</param>
        /// <param name="testResults">soil test results</param>
        /// <param name="config">field configuration</param>
        /// <returns>date to start schedulling</returns>
        public static DateTime startSchedullingDate(Dictionary<DateTime, double> fert, Dictionary<DateTime, double> testResults, Config config)
        {
            //Make all the necessary data structures
            DateTime[] cropDates = Functions.DateSeries(config.Current.EstablishDate, config.Current.HarvestDate);
            DateTime startSchedulleDate = config.Current.EstablishDate; //Earliest start to schedulling is establishment date
            if (testResults.Keys.Count > 0)
                if (testResults.Keys.Last() > config.Current.EstablishDate) //If test results specified after establishment that becomes start of schedulling date
                    startSchedulleDate = testResults.Keys.Last();
            DateTime lastFertDate = new DateTime();
            foreach (DateTime d in fert.Keys)
            {
                if (fert[d] > 0)
                    lastFertDate = d;
            }
            if (lastFertDate > startSchedulleDate)
                startSchedulleDate = lastFertDate;  //If Fertiliser already applied after last test date them last fert date becomes start of scheudlling date
            startSchedulleDate = startSchedulleDate.AddDays(1); //Start schedule the day after the last test or application
            return startSchedulleDate; 
        }
        
        /// <summary>
        /// Adds specified establishment fert to the soil N then determines how much additional fertiliser N is required and when the crop will need it.
        /// </summary>
        /// <param name="fertiliserN">Date indexed series of fertiliser applied</param>
        /// <param name="soilN">Date indexed series of soil N corrected for test values, passed as ref so scheduled fertiliser is added to this property</param>
        /// <param name="lostN">Date indexed series of N losses from leaching or gasious</param>
        /// <param name="residueMin">Date indexed series of daily mineralisation from residues</param>
        /// <param name="somN">Date indexed series of daily mineralisation from soil organic matter</param>
        /// <param name="cropN">Date indexed series of standing crop N</param>
        /// <param name="testResults">Date indexed set of test values</param>
        /// <returns></returns>
        public static void RemainingFertiliserSchedule(DateTime startSchedulleDate,DateTime endScheduleDate,
                                                       ref SimulationType thisSim)
        {
            Config config = thisSim.config;
            DateTime[] schedullingDates = Functions.DateSeries(startSchedulleDate, endScheduleDate);

            // Set other variables needed to derive fertiliser requirement
            //double CropN = cropN[config.Current.HarvestDate] - cropN[startSchedulleDate];
            double trigger = thisSim.config.Field.Trigger;

            int remainingSplits = thisSim.config.Field.Splits;

            // Determine dates that each fertiliser application should be made
            foreach (DateTime d in schedullingDates)
            {
                if (thisSim.SoilN[d] < trigger)
                {
                    double initialN = thisSim.SoilN[d];
                    double losses = 0;
                    double NAppn = 0;
                    if (remainingSplits > 0)
                    {
                        for (int passes = 0; passes < 50; passes++)
                        {
                            double lastPassLossEst = losses;
                            double remainingReqN = remainingRequirement(d, endScheduleDate, thisSim) + losses;
                            NAppn = remainingReqN / remainingSplits;
                            double newSoiN = initialN + NAppn;
                            SoilNitrogen.UpdateBalance(d, newSoiN, ref thisSim);
                            losses = anticipatedLosses(d, endScheduleDate, thisSim.LostN);
                            double lossChange = losses - lastPassLossEst;
                            if (lossChange < 0.5)
                                break;
                        }
                        thisSim.FertiliserN[d] += NAppn;
                        remainingSplits -= 1;
                    }
                    else
                    {
                    //    NAppn = remainingRequirement(d, endScheduleDate, thisSim); 
                    //    double newSoiN = thisSim.SoilN[d] + NAppn;
                    //    SoilNitrogen.UpdateBalance(d, newSoiN, ref thisSim);
                    //    thisSim.FertiliserN[d] += NAppn;
                    }
                }
            }
        }

        private static double remainingRequirement(DateTime startDate, DateTime endDate, SimulationType thisSim)
        {
            double remainingCropN = thisSim.CropN[endDate] - thisSim.CropN[startDate];
            DateTime[] remainingDates = Functions.DateSeries(startDate, endDate);
            double remainingOrgN = remainingMineralisation(remainingDates, thisSim.NResidues, thisSim.NSoilOM);
            return Math.Max(0, remainingCropN - remainingOrgN);
        }

        private static double remainingMineralisation(DateTime[] remainingDates, Dictionary<DateTime, double> residueMin, Dictionary<DateTime, double> somN)
        {
            double mineralisation = 0;
            foreach (DateTime d in remainingDates)
            {
                mineralisation += residueMin[d];
                mineralisation += somN[d];
            }
            return mineralisation;
        }

        private static double anticipatedLosses(DateTime startDate, DateTime endDate, Dictionary<DateTime, double> lostN)
        {
            DateTime[] remainingDates = Functions.DateSeries(startDate, endDate);
            double losses = 0;
            foreach (DateTime d in remainingDates)
            {
                losses += lostN[d];
            }
            return losses;
        }

            public static void ApplyExistingFertiliser(DateTime startApplicationDate, DateTime endApplicationDate, 
                                                   Dictionary<DateTime, double> appliedN,
                                                   ref SimulationType thisSim)
            
        {
            DateTime[] applicationDates = Functions.DateSeries(startApplicationDate, endApplicationDate);

            foreach (DateTime d in applicationDates)
            {
                if (appliedN.ContainsKey(d))
                {
                    //AddFertiliser(ref soilN, appliedN[d], d, config);
                    thisSim.FertiliserN[d] = appliedN[d];
                    //Losses.UpdateLosses(d, config.Following.HarvestDate, ref soilN, ref lostN, drainage, config);
                    double todaySoilN = thisSim.SoilN[d] + appliedN[d];
                    SoilNitrogen.UpdateBalance(d, todaySoilN, ref thisSim); 
                }
            }
        }
    }
}
