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
    public class SoilNitrogen
    {
        /// <summary>
        /// Calculates soil mineral nitrogen from an assumed initial value and modeled crop uptake and mineralisation from residues and soil organic matter
        /// </summary>
        /// <param name="uptake">series of daily N uptake values over the duration of the rotatoin</param>
        /// <param name="residue">series of mineral N released daily to the soil from residue mineralisation</param>
        /// <param name="som">series of mineral N released daily to the soil from organic matter</param>
        /// <returns>date indexed series of estimated soil mineral N content</returns>
        public static void UpdateBalance(DateTime updateDate,
                                          ref SimulationType thisSim, bool fertSchedullingOn)
        {
            if (updateDate == thisSim.config.StartDate)
            {
                //Start at zero on initial day
                thisSim.SoilN[updateDate] = 0;
            }
            else
            {
                //Fertiliser iterates through this multiple times so need to set start soil N back to value at start of itterations
                thisSim.SoilN[updateDate] = thisSim.SoilN[updateDate.AddDays(-1)]; 
            }
            
            DateTime[] updateDates = Functions.DateSeries(updateDate, thisSim.config.Following.HarvestDate);
            foreach (DateTime d in updateDates)
            {
                if ((thisSim.NFertiliserReleased[d] > 0)||(thisSim.ResetDeltaN[d] !=0))
                {
                    thisSim.SoilN[d] = thisSim.SoilN[d.AddDays(-1)] + thisSim.NFertiliserReleased[d] + thisSim.ResetDeltaN[d];
                }
                else
                {
                    thisSim.SoilN[d] = thisSim.SoilN[d.AddDays(-1)];
                }

                thisSim.SoilN[d] += thisSim.NSoilOM[d]; //add Som mineralisation
                double rootExtractionFactor = Math.Max(0.1, Math.Min(1, thisSim.RootDepth[d] / 0.3)) * 0.2;//20% of soil N can be used in a day if roots are deeper than 30cm
                double plantAvailableN = thisSim.SoilN[d] * rootExtractionFactor;
                double microbeAvailableN = thisSim.SoilN[d] * 0.2;
                double potentialImobilisation = Math.Max(0, thisSim.NResidues[d] * -1); //if NResidues is negative imobilisatin is happening 
                if (potentialImobilisation == 0)
                {
                    thisSim.SoilN[d] += thisSim.NResidues[d]; // If imobilisation not happening add mineralisation from residues to soil
                    plantAvailableN = thisSim.SoilN[d] * rootExtractionFactor;  //and recalculate available soil N to account for residue mineralisation 
                    microbeAvailableN = thisSim.SoilN[d] * 0.2;
                }
                double potentialCropUptake = thisSim.NUptake[d];
                double potentialUptake = potentialCropUptake + potentialImobilisation;
                double actualCropUptake = potentialCropUptake;  //Start with uptake at potential and revise down if shortage
                double actualImobilisation = potentialImobilisation; //Start with uptake at potential and revise down if shortage
                // Note: N shortage constraint only applies when ScheduleFert=false
                // When scheduling is on (true), shortages are NOT constrained here;
                // the scheduler compensates by recommending additional fertilizer
                // This allows dynamic scheduling to work around supply constraints
                if (((potentialUptake > microbeAvailableN) || (potentialCropUptake > plantAvailableN)) && (fertSchedullingOn == false)) //Is there a shortage  Only constrain crop N uptake if tests are being run.  For schedulling to work need to have crop uptake unconstrained
                {
                    double propnCropPotUptake = 0;
                    propnCropPotUptake = potentialCropUptake / potentialUptake;  //What proportion of the limited N will the crop get based on its relative demand
                    actualCropUptake = plantAvailableN * propnCropPotUptake;
                    double CropNshortage = potentialCropUptake - actualCropUptake;
                    thisSim.CropShortageN[d] = CropNshortage;
                    if (CropNshortage > 0)
                    {
                        Crop.ConstrainNUptake(ref thisSim, CropNshortage, d); //Reduce Crop uptake below potential
                    }
                    actualImobilisation = Math.Min(potentialImobilisation, microbeAvailableN - actualCropUptake);  //What proporiton of the limited N will residue imobilisation get based on its relative demand
                    if (actualImobilisation > 0)
                    {
                        thisSim.NResidues[d] = -actualImobilisation; //Reduce imobilisation below potential
                    }
                }
                thisSim.SoilN[d] -= actualCropUptake;  //Remove actual crop uptake from soil
                thisSim.SoilN[d] -= actualImobilisation; //Remove actual imobilisaiton from soil.  This will be zero if mineralisation is occuring.

                double newLossEstimate = Losses.DailyLoss(d, thisSim);
                thisSim.NLost[d] = newLossEstimate;
                thisSim.SoilN[d] -= newLossEstimate;//(newLossEstimate - lossAlreadyCountedPriorToSet);
                //resetN -= lossAlreadyCountedPriorToSet;

                CheckNBalance todayCheck = new CheckNBalance(initSoilN: thisSim.SoilN[d.AddDays(-1)],
                                               initStandingCropN: thisSim.CropN[d.AddDays(-1)],
                                               dtransPlantN: thisSim.NTransPlant[d],
                                               dResidueN: thisSim.NResidues[d],
                                               dSOMN: thisSim.NSoilOM[d],
                                               dResetN: thisSim.NFertiliserReleased[d],
                                               finalMinearlN: thisSim.SoilN[d],
                                               standingCropN: thisSim.CropN[d],
                                               dExportN: thisSim.ExportN[d],
                                               dLostN: thisSim.NLost[d],
                                               dFertiliserN: thisSim.ResetDeltaN[d] );
            }

        }

        /// <summary>
        /// Takes soil mineral N test values and adjustes to predicted N balance to correspond with these values on their specific dates
        /// </summary>
        /// <param name="testResults">date indexed series of test results</param>
        /// <param name="soilN">date indexed series of soil mineral N estimates to be corrected with measurements.  Passed in as ref so 
        /// <param name="nApplied">nitrogen fertiliser already applied</param>
        /// the corrections are applied to the property passed in</param>
        public static void TestsAndActualFertiliser(Dictionary<DateTime, double> testResults, ref SimulationType thisSim, Dictionary<DateTime, double> nApplied, bool ScheduleFert)
        {
            List<DateTime> UpdateDates = testResults.Keys.ToList();
            UpdateDates.AddRange(nApplied.Keys.ToList());
            UpdateDates = UpdateDates.Distinct().ToList();
            UpdateDates.Sort((a, b) => a.CompareTo(b));
            

            foreach (DateTime d in UpdateDates)
            {
                if (testResults.ContainsKey(d))  //Set soil N on days of tests
                {
                    double dCorrection = testResults[d] - thisSim.SoilN[d];
                    thisSim.ResetDeltaN[d] = dCorrection;
                    SoilNitrogen.UpdateBalance(d, ref thisSim, ScheduleFert); 
                }
                if (nApplied.ContainsKey(d))  //Update soil N on days of fertiliser application
                {
                    if (!testResults.ContainsKey(d)) // Dont add fertiliser if soil test was entered on the same day
                    {
                        Fertiliser.SetFertiliserRelease(nApplied[d], d, thisSim);
                        SoilNitrogen.UpdateBalance(d, ref thisSim, ScheduleFert);
                    }
                    thisSim.NFertiliserApplied[d] = nApplied[d];
                }
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
                return initialN + initialStandingCropN + dTransplantN + dResidueN + dSOMN + dResetN + dFertiliserN;
            }
        }
        private double initialN { get; set; }
        private double initialStandingCropN { get; set; }
        private double dTransplantN { get; set; }
        private double dResidueN { get; set; }
        private double dSOMN { get; set; }
        private double dResetN { get; set; }
        private double dFertiliserN { get; set; }


        /// Out
        public double OUTs
        {
            get
            {
                return finalMinearlN + standingCropN + dLostN + dExportN;
            }
        }
        private double finalMinearlN { get; set; }
        private double standingCropN { get; set; }
        private double dExportN { get; set; }
        private double dLostN { get; set; }
    

        private void doCheck()
        {
            double balanceError = INs - OUTs;
            if (Math.Abs(balanceError) > 0.000001)
                throw new Exception("Mass balance violated");
        }
        
        public CheckNBalance() { }
        public CheckNBalance(double initSoilN, double initStandingCropN, double dtransPlantN, double dResidueN, double dSOMN, double dResetN,
                             double finalMinearlN, double standingCropN, double dExportN,  double dLostN, double dFertiliserN)
        {
            this.initialN = initSoilN;
            this.initialStandingCropN = initStandingCropN;
            this.dTransplantN = dtransPlantN;
            this.dResidueN = dResidueN;
            this.dSOMN = dSOMN;
            this.dResetN = dResetN;
            this.finalMinearlN = finalMinearlN;
            this.standingCropN = standingCropN; 
            this.dExportN = dExportN;
            this.dLostN = dLostN;
            this.dFertiliserN = dFertiliserN;

            doCheck();
        }
    }

}
