// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using SVSModel.Configuration;
using SVSModel.Models;

namespace SVSModel.Simulation
{
    public class Simulation
    {
        public static SimulationType thisSim = null;

        /// <summary>
        /// Function that steps through each of the components of the N balance for a crop rotation and returns the results in 2D array format
        /// </summary>
        /// <param name="meanT">A date indexed dictionary of daily mean temperatures</param>
        /// <param name="meanRain">A date indexed dictionary of daily mean rainfall</param>
        /// <param name="meanPET">A date indexed dictionary of daily mean potential evapotranspiration</param>
        /// <param name="testResults">A date indexed dictionary with soil mineral N test results</param>
        /// <param name="nAapplied">A date indexed dictionary of N already applied (or planned) </param>
        /// <param name="config">A specific class that holds all the simulation configuration data in the correct types for use in the model</param>
        /// <returns>A 2D array of N balance variables</returns>
        public static object[,] SimulateField(Dictionary<DateTime, double> meanT,
            Dictionary<DateTime, double> meanRain,
            Dictionary<DateTime, double> meanPET,
            Dictionary<DateTime, double> testResults,
            Dictionary<DateTime, double> nAapplied,
            Config config,
            double initialN,
            bool ScheduleFert = true)
        {
            DateTime[] simDates = Functions.DateSeries(config.StartDate.AddDays(-1), config.EndDate);

            thisSim = new SimulationType(simDates, config, meanT, meanRain, meanPET);

            //Apply inCropRainfallFactor to Rain
            thisSim.meanRain = Functions.ApplyRainfallFactor(meanRain, config);

            //Run crop model for each crop in rotation to calculate CropN (total standing in in crop) and Nuptake (Daily N removal from the soil by the crop)
            foreach (CropConfig crop in config.Rotation) //Step through each crop position
            {
                //Calculated outputs for each crop
                CropType currentCrop = Crop.Grow(meanT, crop);

                foreach (DateTime d in currentCrop.growDates)
                {
                    if (d >= simDates[0])
                    {
                        thisSim.NDemand[d] = currentCrop.TotalCropN[d];
                        thisSim.NUptake[d] = currentCrop.CropUptakeN[d];
                        thisSim.CropN[d] = currentCrop.TotalCropN[d];
                        thisSim.ProductN[d] = currentCrop.SaleableProductN[d];
                        thisSim.Cover[d] = currentCrop.Cover[d];
                        if (d == crop.EstablishDate)
                            thisSim.NTransPlant[d] = currentCrop.TotalCropN[d];
                        if (d == crop.HarvestDate)
                            thisSim.ExportN[d.AddDays(1)] = currentCrop.TotalCropN[d];
                    }

                    currentCrop.TotalNDemand = currentCrop.TotalCropN;
                }

                crop.SimResults = currentCrop;
                crop.ResRoot = crop.SimResults.RootN[crop.HarvestDate];
                crop.ResStover = crop.SimResults.StoverN[crop.HarvestDate];
                crop.ResFieldLoss = crop.SimResults.FieldLossN[crop.HarvestDate];
                crop.NUptake = crop.SimResults.TotalCropN[crop.HarvestDate];
                crop.NDemand = crop.SimResults.TotalNDemand[crop.HarvestDate];
            }

            // Calculate soil water content and drainage
            SoilWater.Balance(ref thisSim);

            // Calculate residue mineralisation
            Residues.Mineralisation(ref thisSim);

            // Calculate soil OM mineralisation
            SoilOrganic.Mineralisation(ref thisSim);

            //Do initial nitorgen balance with actual fertiliser but no scheduled fertiliser or resets
            SoilNitrogen.UpdateBalance(config.StartDate, initialN, 0, 0, ref thisSim, false, new Dictionary<DateTime, double>(), ScheduleFert);

            //Reset soil N with test valaues
            SoilNitrogen.TestsAndActualFertiliser(testResults, ref thisSim, nAapplied);

            //Calculate Fertiliser requirements and add into soil N
            DateTime StartSchedullingDate = Fertiliser.startSchedullingDate(nAapplied, testResults, config);
            DateTime EndSchedullingDate = config.Current.HarvestDate;
            Fertiliser.RemainingFertiliserSchedule(StartSchedullingDate, EndSchedullingDate, ref thisSim);

            doNbalanceSummary(ref thisSim);

            //Pack Daily State Variables into a 2D array so they can be output
            object[,] outputs = new object[simDates.Length + 1, 14];

            outputs[0, 0] = "Date"; Functions.packRows(0, simDates, ref outputs);
            outputs[0, 1] = "SoilMineralN"; Functions.packRows(1, thisSim.SoilN, ref outputs);
            outputs[0, 2] = "UptakeN"; Functions.packRows(2, thisSim.NUptake, ref outputs);
            outputs[0, 3] = "ResidueN"; Functions.packRows(3, thisSim.NResidues, ref outputs);
            outputs[0, 4] = "SoilOMN"; Functions.packRows(4, thisSim.NSoilOM, ref outputs);
            outputs[0, 5] = "FertiliserN"; Functions.packRows(5, thisSim.NFertiliser, ref outputs);
            outputs[0, 6] = "CropN"; Functions.packRows(6, thisSim.CropN, ref outputs);
            outputs[0, 7] = "ProductN"; Functions.packRows(7, thisSim.ProductN, ref outputs);
            outputs[0, 8] = "LostN"; Functions.packRows(8, thisSim.NLost, ref outputs);
            outputs[0, 9] = "RSWC"; Functions.packRows(9, thisSim.RSWC, ref outputs);
            outputs[0, 10] = "Drainage"; Functions.packRows(10, thisSim.Drainage, ref outputs);
            outputs[0, 11] = "Irrigation"; Functions.packRows(11, thisSim.Irrigation, ref outputs);
            outputs[0, 12] = "Green cover"; Functions.packRows(12, thisSim.Cover, ref outputs);
            outputs[0, 13] = "NDemand"; Functions.packRows(13, thisSim.NDemand, ref outputs);

            return outputs;
        }

        private static void doNbalanceSummary(ref SimulationType thisSim)
        {
            DateTime Start = thisSim.config.Current.EstablishDate;
            DateTime End = thisSim.config.Current.HarvestDate;

            CropNBalanceSummary CurrentNBalanceSummary = new CropNBalanceSummary(
                mineralIn: thisSim.SoilN[Start],
                cropIn: Functions.sumOverDates(Start, End, thisSim.NTransPlant),
                residueIn: Functions.sumOverDates(Start, End, thisSim.NResidues),
                sOMIn: Functions.sumOverDates(Start, End, thisSim.NSoilOM),
                fertiliserIn: Functions.sumOverDates(Start, End, thisSim.NFertiliser),
                mineralOut: thisSim.SoilN[End],
                productOut: thisSim.ProductN[End],
                stoverOut: thisSim.CropN[End] - thisSim.ProductN[End],
                lossesOut: Functions.sumOverDates(Start, End, thisSim.NLost));
            thisSim.CurrentNBalanceSummary = CurrentNBalanceSummary;
        }
    }

    public class SimulationType
    {
        public Config config;
        public DateTime[] simDates;
        public Dictionary<DateTime, double> meanT;
        public Dictionary<DateTime, double> meanRain;
        public Dictionary<DateTime, double> meanPET;
        public Dictionary<DateTime, double> NTransPlant;
        public Dictionary<DateTime, double> NDemand;
        public Dictionary<DateTime, double> NUptake;
        public Dictionary<DateTime, double> CropN;
        public Dictionary<DateTime, double> ProductN;
        public Dictionary<DateTime, double> Cover;
        public Dictionary<DateTime, double> RSWC;
        public Dictionary<DateTime, double> Drainage;
        public Dictionary<DateTime, double> Irrigation;
        public Dictionary<DateTime, double> NResidues;
        public Dictionary<DateTime, double> NSoilOM;
        public Dictionary<DateTime, double> NLost;
        public Dictionary<DateTime, double> NFertiliser;
        public Dictionary<DateTime, double> SoilN;
        public Dictionary<DateTime, double> ExportN;
        public Dictionary<DateTime, double> CropShortageN;
        public CropNBalanceSummary CurrentNBalanceSummary;

        public SimulationType(
            DateTime[] _simDates,
            Config _config,
            Dictionary<DateTime, double> _meanT,
            Dictionary<DateTime, double> _meanRain,
            Dictionary<DateTime, double> _meanPET)
        {
            config = _config;
            simDates = _simDates;
            meanT = _meanT;
            meanRain = _meanRain;
            meanPET = _meanPET;
            NTransPlant = Functions.dictMaker(simDates, new double[simDates.Length]);
            NDemand = Functions.dictMaker(simDates, new double[simDates.Length]);
            NUptake = Functions.dictMaker(simDates, new double[simDates.Length]);
            CropN = Functions.dictMaker(simDates, new double[simDates.Length]);
            ProductN = Functions.dictMaker(simDates, new double[simDates.Length]);
            Cover = Functions.dictMaker(simDates, new double[simDates.Length]);
            RSWC = Functions.dictMaker(simDates, new double[simDates.Length]);
            Drainage = Functions.dictMaker(simDates, new double[simDates.Length]);
            Irrigation = Functions.dictMaker(simDates, new double[simDates.Length]);
            NResidues = Functions.dictMaker(simDates, new double[simDates.Length]);
            NSoilOM = Functions.dictMaker(simDates, new double[simDates.Length]);
            NLost = Functions.dictMaker(simDates, new double[simDates.Length]);
            NFertiliser = Functions.dictMaker(simDates, new double[simDates.Length]);
            SoilN = Functions.dictMaker(simDates, new double[simDates.Length]);
            ExportN = Functions.dictMaker(simDates, new double[simDates.Length]);
            CropShortageN = Functions.dictMaker(simDates, new double[simDates.Length]);
        }
    }

    public class CropNBalanceSummary
    {
        public double MineralIn { get; }
        public double CropIn { get; }
        public double ResidueIn { get; }
        public double SOMIn { get; }
        public double FertiliserIn { get; }

        public double UncharacterisedIn
        {
            get { return Math.Max(0, Outs - Ins); }
        }

        /// Out
        public double MinearlOut { get; }

        public double ProductOut { get; }
        public double StoverOut { get; }
        public double LossesOut { get; }

        public double UncharacterisedOut
        {
            get { return Math.Max(0, Ins - Outs); }
        }

        public double Ins
        {
            get { return MineralIn + CropIn + ResidueIn + SOMIn + FertiliserIn; }
        }

        public double Outs
        {
            get { return MinearlOut + ProductOut + StoverOut + LossesOut; }
        }

        public double balance
        {
            get { return Ins - Outs; }
        }

        public CropNBalanceSummary(double mineralIn, double cropIn, double residueIn, double sOMIn, double fertiliserIn,
            double mineralOut, double productOut, double stoverOut, double lossesOut)
        {
            MineralIn = mineralIn;
            CropIn = cropIn;
            ResidueIn = residueIn;
            SOMIn = sOMIn;
            FertiliserIn = fertiliserIn;
            MinearlOut = mineralOut;
            ProductOut = productOut;
            StoverOut = stoverOut;
            LossesOut = lossesOut;
        }
    }

    public class NBalanceSummary
    {
        public Dictionary<string, int> Mineral { get; }
        public Dictionary<string, int> CropProduct { get; }
        public Dictionary<string, int> OtherCropParts { get; }
        public Dictionary<string, int> SoilOrganic { get; }
        public Dictionary<string, int> Residues { get; }
        public Dictionary<string, int> Fertiliser { get; }
        public Dictionary<string, int> UnCharacterised { get; }
        public Dictionary<string, int> Total { get; }

        public NBalanceSummary(CropNBalanceSummary nBalance)
        {
            var resMin = Math.Max(0, nBalance.ResidueIn);
            var resImob = Math.Max(0, -nBalance.ResidueIn);
            var unCharOut = nBalance.UncharacterisedOut + nBalance.LossesOut;
            Mineral = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(nBalance.MineralIn, 0) },
                { "Out", (int)Math.Round(nBalance.MinearlOut, 0) }
            };
            CropProduct = new Dictionary<string, int>
            {
                { "In", 0 },
                { "Out", (int)Math.Round(nBalance.ProductOut, 0) }
            };
            OtherCropParts = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(nBalance.CropIn, 0) },
                { "Out", (int)Math.Round(nBalance.StoverOut, 0) }
            };
            SoilOrganic = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(nBalance.SOMIn, 0) },
                { "Out", 0 }
            };
            Residues = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(resMin, 0) },
                { "Out", (int)Math.Round(resImob, 0) }
            };
            Fertiliser = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(nBalance.FertiliserIn, 0) },
                { "Out", 0 }
            };
            UnCharacterised = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(nBalance.UncharacterisedIn, 0) },
                { "Out", (int)Math.Round(unCharOut, 0) }
            };
            Total = new Dictionary<string, int>
            {
                { "In", (int)Math.Round(nBalance.Ins) },
                { "Out", (int)Math.Round(nBalance.Outs) }
            };
        }
    }
}