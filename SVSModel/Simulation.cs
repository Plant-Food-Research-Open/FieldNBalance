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
                                                      Config config)
        {
            
            
            DateTime[] simDates = Functions.DateSeries(config.Prior.HarvestDate.AddDays(-1), config.Following.HarvestDate);

            thisSim = new SimulationType(simDates, config, meanT, meanRain, meanPET);

            //Apply inCropRainfallFactor to Rain
            thisSim.meanRain = Functions.ApplyRainfallFactor(meanRain, config);

            //Run crop model for each crop in rotation to calculate CropN (total standing in in crop) and Nuptake (Daily N removal from the soil by the crop)
            //Dictionary<DateTime, double> NUptake = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> CropN = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> ProductN = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> Cover = Functions.dictMaker(simDates, new double[simDates.Length]);

            foreach (CropConfig crop in config.Rotation) //Step through each crop position
            {
                //Make date series for duraion of the crop and accumulate thermal time over that period
                DateTime[] cropDates = Functions.DateSeries(crop.EstablishDate, crop.HarvestDate);
                Dictionary<DateTime, double> AccTt = Functions.AccumulateTt(cropDates, meanT);

                //Calculated outputs for each crop
                object[,] cropsOutPuts = Crop.Grow(AccTt, crop);

                //Pack Crop N and N uptake results for each crop into the corresponding variables for the rotation (i.e stick all crops together to form the rotation)
                Dictionary<DateTime, double> cropsNUptake = Functions.dictMaker(cropsOutPuts, "CropUptakeN");
                Dictionary<DateTime, double> totalCropN = Functions.dictMaker(cropsOutPuts, "TotalCropN");
                Dictionary<DateTime, double> productN = Functions.dictMaker(cropsOutPuts, "SaleableProductN");
                Dictionary<DateTime, double> cover = Functions.dictMaker(cropsOutPuts, "Cover");
                foreach (DateTime d in cropsNUptake.Keys)
                {
                    if (d >= simDates[0])
                    {
                        thisSim.NUptake[d] = cropsNUptake[d];
                        thisSim.CropN[d] = totalCropN[d];
                        thisSim.ProductN[d] = productN[d];
                        thisSim.Cover[d] = cover[d];
                    }
                }

                //Pack final crop variables to field config dict for use in other parts of the N balance
                crop.ResRoot = Functions.GetFinal(cropsOutPuts, "RootN");
                crop.ResStover = Functions.GetFinal(cropsOutPuts, "StoverN") * crop.ResidueFactRetained;
                crop.ResFieldLoss = Functions.GetFinal(cropsOutPuts, "FieldLossN") * crop.ResidueFactRetained;
                crop.NUptake = Functions.GetFinal(cropsOutPuts, "TotalCropN");
            }

            // Calculate soil water content and drainage
            //Dictionary<DateTime, double> RSWC = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> Drainage = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> Irrigation = Functions.dictMaker(simDates, new double[simDates.Length]);
            SoilWater.Balance(ref thisSim);

            // Calculate residue mineralisation
            Residues.Mineralisation(ref thisSim);

            // Calculate soil OM mineralisation
            SoilOrganic.Mineralisation(ref thisSim);

            // Start with zero values for soilN, loss and fertiliser
            //Dictionary<DateTime, double> LostN = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> FertiliserN = Functions.dictMaker(simDates, new double[simDates.Length]);
            //Dictionary<DateTime, double> SoilN = Functions.dictMaker(simDates, new double[simDates.Length]);

            //Do initial nitorgen balance with no fertiliser or resets
            SoilNitrogen.UpdateBalance(config.Prior.HarvestDate, config.Field.InitialN, ref thisSim);

            //Add fertiliser that has already been applied to the N balance
            DateTime StartApplicationDate = config.Prior.HarvestDate;
            DateTime StartSchedullingDate = Fertiliser.startSchedullingDate(nAapplied, testResults, config);
            Fertiliser.ApplyExistingFertiliser(StartApplicationDate, StartSchedullingDate, nAapplied, ref thisSim);

            //Reset soil N with test valaues
            SoilNitrogen.TestCorrection(testResults, ref thisSim);

            //Calculate Fertiliser requirements and add into soil N
            DateTime EndSchedullingDate = config.Current.HarvestDate;
            Fertiliser.RemainingFertiliserSchedule(StartSchedullingDate, EndSchedullingDate, ref thisSim);

            //Pack Daily State Variables into a 2D array so they can be output
            object[,] outputs = new object[simDates.Length + 1, 13];

            outputs[0, 0] = "Date"; Functions.packRows(0, simDates, ref outputs);
            outputs[0, 1] = "SoilMineralN"; Functions.packRows(1, thisSim.SoilN, ref outputs);
            outputs[0, 2] = "UptakeN"; Functions.packRows(2, thisSim.NUptake, ref outputs);
            outputs[0, 3] = "ResidueN"; Functions.packRows(3, thisSim.NResidues, ref outputs);
            outputs[0, 4] = "SoilOMN"; Functions.packRows(4, thisSim.NSoilOM, ref outputs);
            outputs[0, 5] = "FertiliserN"; Functions.packRows(5, thisSim.FertiliserN, ref outputs);
            outputs[0, 6] = "CropN"; Functions.packRows(6, thisSim.CropN, ref outputs);
            outputs[0, 7] = "ProductN"; Functions.packRows(7, thisSim.ProductN, ref outputs);
            outputs[0, 8] = "LostN"; Functions.packRows(8, thisSim.LostN, ref outputs);
            outputs[0, 9] = "RSWC"; Functions.packRows(9, thisSim.RSWC, ref outputs);
            outputs[0, 10] = "Drainage"; Functions.packRows(10, thisSim.Drainage, ref outputs);
            outputs[0, 11] = "Irrigation"; Functions.packRows(11, thisSim.Irrigation, ref outputs);
            outputs[0, 12] = "Green cover"; Functions.packRows(12, thisSim.Cover, ref outputs);

            return outputs;
        }
    }
    public class SimulationType
    {
        public Config config;
        public DateTime[] simDates;
        public Dictionary<DateTime, double> meanT;
        public Dictionary<DateTime, double> meanRain;
        public Dictionary<DateTime, double> meanPET;
        public Dictionary<DateTime, double> NUptake;
        public Dictionary<DateTime, double> CropN;
        public Dictionary<DateTime, double> ProductN;
        public Dictionary<DateTime, double> Cover;
        public Dictionary<DateTime, double> RSWC;
        public Dictionary<DateTime, double> Drainage;
        public Dictionary<DateTime, double> Irrigation;
        public Dictionary<DateTime, double> NResidues;
        public Dictionary<DateTime, double> NSoilOM;
        public Dictionary<DateTime, double> LostN;
        public Dictionary<DateTime, double> FertiliserN;
        public Dictionary<DateTime, double> SoilN;

        public SimulationType(DateTime[] _simDates, Config _config,
                              Dictionary<DateTime, double> _meanT,
                              Dictionary<DateTime, double> _meanRain,
                              Dictionary<DateTime, double> _meanPET) 
        {
            config = _config;
            simDates = _simDates;
            meanT = _meanT;
            meanRain = _meanRain;
            meanPET = _meanPET;
            NUptake = Functions.dictMaker(simDates, new double[simDates.Length]);
            CropN = Functions.dictMaker(simDates, new double[simDates.Length]);
            ProductN = Functions.dictMaker(simDates, new double[simDates.Length]);
            Cover = Functions.dictMaker(simDates, new double[simDates.Length]);
            RSWC = Functions.dictMaker(simDates, new double[simDates.Length]);
            Drainage = Functions.dictMaker(simDates, new double[simDates.Length]);
            Irrigation = Functions.dictMaker(simDates, new double[simDates.Length]);
            NResidues = Functions.dictMaker(simDates, new double[simDates.Length]);
            NSoilOM = Functions.dictMaker(simDates, new double[simDates.Length]);
            LostN = Functions.dictMaker(simDates, new double[simDates.Length]);
            FertiliserN = Functions.dictMaker(simDates, new double[simDates.Length]);
            SoilN = Functions.dictMaker(simDates, new double[simDates.Length]);
        }

    }
}
