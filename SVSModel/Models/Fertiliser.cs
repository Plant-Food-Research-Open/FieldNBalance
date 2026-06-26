// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

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
                startSchedulleDate = lastFertDate;  //If Fertiliser already applied after last test date then last fert date becomes start of scheudlling date
            startSchedulleDate = startSchedulleDate.AddDays(1); //Start schedule the day after the last test or application
            return startSchedulleDate; 
        }

        /// <summary>
        /// Add up how many fertiliser application splits have been applied prior to the start of schedulling
        /// </summary>
        /// <param name="startSchedulleDate">Date that schedulling starts</param>
        /// <param name="fert">The fertiliser already applied</param>
        /// <param name="config">field configuration</param>
        /// <returns>date to start schedulling</returns>
        public static int splitsAppliedAlready(DateTime startSchedulleDate, Dictionary<DateTime, double> fert, Config config)
        {
            int splitsAppliedAlready = 0;
            DateTime[] datesPassedAlready = Functions.DateSeries(config.Current.EstablishDate, startSchedulleDate);
            foreach (DateTime d in datesPassedAlready)
            {
                if (fert[d] > 0)
                {
                    splitsAppliedAlready += 1;
                }
            }
            return splitsAppliedAlready;
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
        public static void RemainingFertiliserSchedule(DateTime startSchedulleDate, DateTime endScheduleDate,
                                                       ref SimulationType thisSim)
        {
            Config config = thisSim.config;
            
            int splitsApplied = splitsAppliedAlready(startSchedulleDate, thisSim.NFertiliserApplied, config);

            // Calculate N application at planting
            DateTime sowingDate = thisSim.config.Current.EstablishDate;
            if (startSchedulleDate <= sowingDate)
            {
                double plantSowingNRequirement = thisSim.config.Current.NDemand * 0.33;
                double sowingFert = Math.Max(0, plantSowingNRequirement - thisSim.SoilN[sowingDate]);
                if (sowingFert > 0)
                {
                    Fertiliser.SetFertiliserRelease(sowingFert, sowingDate, thisSim);
                    SoilNitrogen.UpdateBalance(sowingDate, ref thisSim, true);
                    thisSim.NFertiliserApplied[sowingDate] += sowingFert;
                    splitsApplied += 1;
                }
            }

            // Set other variables needed to derive fertiliser requirement
            int remainingSplits = Math.Max(1,thisSim.config.Field.Splits - splitsApplied);

            // Determine dates that each fertiliser application should be made
            startSchedulleDate = startSchedulleDate.AddDays(8);  //Move internal schedulling dates forward 8 days as fertiliser is recommended 8 days before trigger is met
            endScheduleDate = endScheduleDate.AddDays(8);  //Move internal schedulling dates forward 8 days as fertiliser is recommended 8 days before trigger is met
            DateTime[] schedullingDates = Functions.DateSeries(startSchedulleDate, endScheduleDate);

            foreach (DateTime d in schedullingDates)
            {
                if (remainingSplits > 0)
                {
                    double trigger = Math.Max(20, thisSim.NUptake[d] * 14);
                    if ((thisSim.SoilN[d] < trigger) || ((d == endScheduleDate) && (remainingSplits > 0)))
                    {
                        if (d == endScheduleDate)
                        {
                            remainingSplits = 1;
                        }
                        double initialN = thisSim.SoilN[d.AddDays(-1)];
                        double losses = 0;
                        double NAppn = 0;
                        for (int passes = 0; passes < 50; passes++)
                        {
                            double lastPassLossEst = losses;
                            trigger = Math.Max(15, thisSim.NUptake[thisSim.config.Current.HarvestDate] * 14);
                            double remainingReqN = remainingRequirement(d, thisSim.config.Current.HarvestDate, thisSim, initialN, trigger) + losses;
                            NAppn = remainingReqN / remainingSplits;
                            Fertiliser.SetFertiliserRelease(NAppn, d.AddDays(-8), thisSim, true);
                            SoilNitrogen.UpdateBalance(d.AddDays(-8), ref thisSim, true);
                            losses = anticipatedLosses(d.AddDays(-8), thisSim.config.Current.HarvestDate, thisSim.NLost);
                            double lossChange = losses - lastPassLossEst;
                            if (lossChange < 0.1)
                                break;
                        }
                        thisSim.NFertiliserApplied[d.AddDays(-8)] += NAppn;
                        remainingSplits -= 1;
                    }
                }
            }
        }

        private static Dictionary<int, double> releasePatttern = new Dictionary<int, double>
        {
            { 6, 0.10 },
            { 7, 0.20 },
            { 8, 0.40 },
            { 9, 0.20 },
            { 10, 0.10 }
        };

        public static void SetFertiliserRelease(double nApplied, DateTime applicationDate, SimulationType thisSim, bool overwrite = false)
        {
            for (int i = 6; i <= 10; i++)
            {
                if (overwrite == false)
                {
                    thisSim.NFertiliserReleased[applicationDate.AddDays(i)] += nApplied * releasePatttern[i];
                }
                if (overwrite == true)
                {
                    thisSim.NFertiliserReleased[applicationDate.AddDays(i)] = nApplied * releasePatttern[i];
                }
            }
        }

        private static double remainingRequirement(DateTime startDate, DateTime endDate, SimulationType thisSim, double initialN, double trigger)
        {
            double remainingCropN = thisSim.CropN[endDate] - thisSim.CropN[startDate];
            DateTime[] remainingDates = Functions.DateSeries(startDate, endDate);
            double remainingOrgN = remainingMineralisation(remainingDates, thisSim.NResidues, thisSim.NSoilOM, thisSim.NDemand);
            double surplussMineralN = initialN - trigger;
            return Math.Max(0, remainingCropN - remainingOrgN - surplussMineralN);
        }

        private static double remainingMineralisation(DateTime[] remainingDates, Dictionary<DateTime, double> residueMin, Dictionary<DateTime, double> somN, Dictionary<DateTime, double> NDemand)
        {
            double MineralNUsable = 0;
            foreach (DateTime d in remainingDates)
            {
                double MinearlNAvailable = residueMin[d] + somN[d]; //This is how much minearl N the crop may take up
                MineralNUsable = Math.Min(MinearlNAvailable, NDemand[d]); //Crop can not take up Available N if demand is less that what is available 
            }
            return MineralNUsable;
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
                    //thisSim.NFertiliser[d] = appliedN[d];
                    //SoilNitrogen.UpdateBalance(d, appliedN[d], thisSim.SoilN[d], thisSim.NLost[d],ref thisSim, true); 
                }
            }
        }
    }
}
