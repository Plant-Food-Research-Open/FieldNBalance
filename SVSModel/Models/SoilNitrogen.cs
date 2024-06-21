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
        public static void UpdateBalance(DateTime updateDate, double dResetN, double preSetSoilN, double lossAlreadyCountedPriorToSet, ref SimulationType thisSim, bool IsSet, Dictionary<DateTime, double> nAapplied, bool scheduleFert)
        {

            thisSim.SoilN[updateDate] = preSetSoilN; //Fertiliser iterates through this multiple times so need to set start soil N back to value at start of itterations
            DateTime[] updateDates = Functions.DateSeries(updateDate, thisSim.config.Following.HarvestDate);
            foreach (DateTime d in updateDates)
            {
                if (d == updateDate)
                {
                    thisSim.SoilN[d] += dResetN;
                }
                else
                {
                    thisSim.SoilN[d] = thisSim.SoilN[d.AddDays(-1)];
                }

                if (IsSet == false)
                {
                    thisSim.SoilN[d] += thisSim.NSoilOM[d]; //add Som mineralisation
                    double availableN = thisSim.SoilN[d] * 0.2; //20% of soil N can be used in a day
                    double potentialImobilisation = Math.Max(0, thisSim.NResidues[d] * -1); //if NResidues is negative imobilisatin is happening 
                    if (potentialImobilisation == 0)
                    {
                        thisSim.SoilN[d] += thisSim.NResidues[d]; // If imobilisation not happening add mineralisation from residues to soil
                        availableN = thisSim.SoilN[d] * 0.2;  //and recalculate available soil N to account for residue mineralisation 
                    }
                    double potentialCropUptake = thisSim.NUptake[d];
                    double potentialUptake = potentialCropUptake + potentialImobilisation;
                    double actualCropUptake = potentialCropUptake;  //Start with uptake at potential and revise down if shortage
                    double actualImobilisation = potentialImobilisation; //Start with uptake at potential and revise down if shortage
                    if ((potentialUptake > availableN)&& (scheduleFert == false)) //Is there a shortage  Only constrain crop N uptake if tests are being run.  For schedulling to work need to have crop uptake unconstrained
                    {
                        double propnCropPotUptake = 0;
                        propnCropPotUptake = potentialCropUptake / potentialUptake;  //What proportion of the limited N will the crop get based on its relative demand
                        actualCropUptake = availableN * propnCropPotUptake;
                        double CropNshortage = potentialCropUptake - actualCropUptake;
                        thisSim.CropShortageN[d] = CropNshortage;
                        if (CropNshortage > 0)
                        {
                            Crop.ConstrainNUptake(ref thisSim, CropNshortage, d); //Reduce Crop uptake below potential
                        }
                        actualImobilisation = availableN * (1 - propnCropPotUptake);  //What proporiton of the limited N will residue imobilisation get based on its relative demand
                        if (actualImobilisation > 0)
                        {
                            thisSim.NResidues[d] = -actualImobilisation; //Reduce imobilisation below potential
                        }
                    }
                    thisSim.SoilN[d] -= actualCropUptake;  //Remove actual crop uptake from soil
                    thisSim.SoilN[d] -= actualImobilisation; //Remove actual imobilisaiton from soil.  This will be zero if mineralisation is occuring.
                }

                double newLossEstimate = Losses.DailyLoss(d, thisSim);
                thisSim.NLost[d] = newLossEstimate;
                thisSim.SoilN[d] -= (newLossEstimate - lossAlreadyCountedPriorToSet);
                //resetN -= lossAlreadyCountedPriorToSet;

                CheckNBalance todayCheck = new CheckNBalance(initSoilN: thisSim.SoilN[d.AddDays(-1)],
                                               initStandingCropN: thisSim.CropN[d.AddDays(-1)],
                                               dtransPlantN: thisSim.NTransPlant[d],
                                               dResidueN: thisSim.NResidues[d],
                                               dSOMN: thisSim.NSoilOM[d],
                                               dResetN: dResetN,
                                               finalMinearlN: thisSim.SoilN[d],
                                               standingCropN: thisSim.CropN[d],
                                               dExportN: thisSim.ExportN[d],
                                               dLostN: thisSim.NLost[d],
                                               dFertiliserN: thisSim.NFertiliser[d]);
                lossAlreadyCountedPriorToSet = 0; //Only discount losses already counted on day of reset
                dResetN = 0; // Reset N only a non zero number on the set day otherwise zero
                IsSet = false; // IsSet only true on the day the set is actioned, needs to be false so full balance is done every other day
            }

        }

        /// <summary>
        /// Takes soil mineral N test values and adjustes to predicted N balance to correspond with these values on their specific dates
        /// </summary>
        /// <param name="testResults">date indexed series of test results</param>
        /// <param name="soilN">date indexed series of soil mineral N estimates to be corrected with measurements.  Passed in as ref so 
        /// <param name="nApplied">nitrogen fertiliser already applied</param>
        /// the corrections are applied to the property passed in</param>
        public static void TestsAndActualFertiliser(Dictionary<DateTime, double> testResults, ref SimulationType thisSim, Dictionary<DateTime, double> nApplied)
        {
            List<DateTime> UpdateDates = testResults.Keys.ToList();
            UpdateDates.AddRange(nApplied.Keys.ToList());
            UpdateDates.Sort((a, b) => a.CompareTo(b));

            foreach (DateTime d in UpdateDates)
            {
                if (nApplied.ContainsKey(d))
                {
                    SoilNitrogen.UpdateBalance(d, nApplied[d], thisSim.SoilN[d], thisSim.NLost[d], ref thisSim, true, nApplied, true); 
                    thisSim.NFertiliser[d] = nApplied[d];
                }
                if (testResults.ContainsKey(d))
                {
                    double dCorrection = testResults[d] - thisSim.SoilN[d];
                    SoilNitrogen.UpdateBalance(d, dCorrection, thisSim.SoilN[d], thisSim.NLost[d], ref thisSim, true, nApplied, true); 
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
