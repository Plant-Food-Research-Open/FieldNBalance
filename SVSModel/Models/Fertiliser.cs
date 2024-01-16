using System;
using System.Collections.Generic;
using System.Linq;
using SVSModel.Configuration;

namespace SVSModel.Models
{
    public class Fertiliser
    {
        /// <summary>
        /// Adds specified establishment fert to the soil N then determines how much additional fertiliser N is required and when the crop will need it.
        /// </summary>
        /// <param name="fert">Date indexed series of fertiliser applied</param>
        /// <param name="soilN">Date indexed series of soil N corrected for test values, passed as ref so scheduled fertiliser is added to this property</param>
        /// <param name="lostN">Date indexed series of N losses from leaching or gasious</param>
        /// <param name="residueMin">Date indexed series of daily mineralisation from residues</param>
        /// <param name="somN">Date indexed series of daily mineralisation from soil organic matter</param>
        /// <param name="cropN">Date indexed series of standing crop N</param>
        /// <param name="testResults">Date indexed set of test values</param>
        /// <returns></returns>
        public static void RemainingFertiliserSchedule(
            ref Dictionary<DateTime, double> fert,
            ref Dictionary<DateTime, double> soilN,
            ref Dictionary<DateTime, double> lostN,
            Dictionary<DateTime, double> residueMin,
            Dictionary<DateTime, double> somN,
            Dictionary<DateTime, double> cropN,
            Dictionary<DateTime, double> testResults,
            Config config)
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
            DateTime[] schedullingDates = Functions.DateSeries(startSchedulleDate, config.Current.HarvestDate);

            //Calculate total N from mineralisatin over the duration of the crop
            double mineralisation = 0;
            double fertToDate = 0;
            foreach (DateTime d in schedullingDates)
            {
                mineralisation += residueMin[d];
                mineralisation += somN[d];
                fertToDate += fert[d];
            }

            // Set other variables needed to derive fertiliser requirement
            double CropN = cropN[config.Current.HarvestDate] - cropN[startSchedulleDate];
            double trigger = config.Field.Trigger;
            double efficiency = config.Field.Efficiency;

            // Calculate total fertiliser requirement and ammount to be applied at each application
            double NFertReq = (CropN + trigger) - soilN[startSchedulleDate] - mineralisation - fertToDate;
            NFertReq = Math.Max(0, NFertReq * 1 / efficiency);

            int splits = config.Field.Splits;
            double NAppn = Math.Ceiling(NFertReq / splits);

            // Determine dates that each fertiliser application should be made
            double FertApplied = 0;
            if (splits > 0)
            {
                foreach (DateTime d in schedullingDates)
                {
                    if ((soilN[d] < trigger) && (FertApplied < NFertReq))
                    {
                        AddFertiliser(ref soilN, NAppn * efficiency, d, config);
                        fert[d] += NAppn;
                        FertApplied += NAppn;
                        lostN[d] = NAppn * (1 - efficiency);
                    }
                }
            }
        }

        public static void ApplyExistingFertiliser(
            ref Dictionary<DateTime, double> fertiliserN,
            ref Dictionary<DateTime, double> soilN,
            ref Dictionary<DateTime, double> lostN,
            Dictionary<DateTime, double> appliedN,
            Config config)
        {
            DateTime startApplicationDate = config.Prior.HarvestDate.AddDays(1); //Earliest start to schedulling is establishment date
            DateTime endApplicationDate = config.Following.HarvestDate;
            //if (testResults.Keys.Count > 0)
            //    startApplicationDate = testResults.Keys.Last().AddDays(1); //If test results specified after establishment that becomes start of schedulling date
            double efficiency = config.Field.Efficiency;
            foreach (DateTime d in appliedN.Keys)
            {
                if ((d >= startApplicationDate)&&(d <= endApplicationDate))
                {
                    AddFertiliser(ref soilN, appliedN[d] * efficiency, d, config);
                    fertiliserN[d] = appliedN[d];
                    lostN[d] = appliedN[d] * (1 - efficiency);
                }

            }
        }

        /// <summary>
        /// function to update series of soil mineral N for dates following N fertiliser application
        /// </summary>
        /// <param name="soilN">Date indexed series of soil mineral N data </param>
        /// <param name="fertN">Amount of fertiliser to apply</param>
        /// <param name="fertDate">Date to apply fertiliser</param>
        /// <param name="config">A specific class that holds all the simulation configuration data in the correct types for use in the model</param>
        public static void AddFertiliser(ref Dictionary<DateTime, double> soilN, double fertN, DateTime fertDate, Config config)
        {
            DateTime[] datesFollowingFert = Functions.DateSeries(fertDate, config.Following.HarvestDate);
            foreach (DateTime d in datesFollowingFert)
            {
                soilN[d] += fertN;
            }
        }
    }
}
