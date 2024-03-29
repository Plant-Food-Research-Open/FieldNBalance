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
        public static Dictionary<DateTime, double> InitialBalance(
            Dictionary<DateTime, double> uptake,
            Dictionary<DateTime, double> residue,
            Dictionary<DateTime, double> som)
        {
            DateTime[] simDates = uptake.Keys.ToArray();
            Dictionary<DateTime, double> soilN = Functions.dictMaker(simDates, new double[simDates.Length]);
            foreach (DateTime d in simDates)
            {
                if (d == simDates[0])
                {
                    soilN[simDates[0]] = Constants.InitialN;
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
        public static void UpdateBalance(DateTime updateDate, double dResetN, double preSetSoilN, double lossAlreadyCountedPriorToSet, ref SimulationType thisSim, bool IsSet)
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
                    thisSim.SoilN[d] += thisSim.NResidues[d];
                    thisSim.SoilN[d] += thisSim.NSoilOM[d];
                    double actualUptake = thisSim.NUptake[d];//Math.Min(thisSim.NUptake[d], thisSim.SoilN[d]*.1);
                                                             //double Nshortage = thisSim.NUptake[d] - actualUptake;
                                                             //if (Nshortage < 0)
                                                             //    Crop.ConstrainNUptake(ref thisSim, Nshortage,d);
                    thisSim.SoilN[d] -= actualUptake;
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
                                               dLostN: thisSim.NLost[d]);
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
        /// the corrections are applied to the property passed in</param>
        public static void TestCorrection(Dictionary<DateTime, double> testResults, ref SimulationType thisSim)
        {
            foreach (DateTime d in testResults.Keys)
            {
                double dCorrection = testResults[d] - thisSim.SoilN[d];
                SoilNitrogen.UpdateBalance(d, dCorrection, thisSim.SoilN[d] - thisSim.NFertiliser[d], thisSim.NLost[d], ref thisSim, true); //need to take out fertiliser if fert applied on same day as test so it doesn't break balance check test
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
                return initialN + initialStandingCropN + dTransplantN + dResidueN + dSOMN + dResetN;
            }
        }
        private double initialN { get; set; }
        private double initialStandingCropN { get; set; }
        private double dTransplantN { get; set; }
        private double dResidueN { get; set; }
        private double dSOMN { get; set; }
        private double dResetN { get; set; }


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
                             double finalMinearlN, double standingCropN, double dExportN,  double dLostN)
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

            doCheck();
        }
    }

}
