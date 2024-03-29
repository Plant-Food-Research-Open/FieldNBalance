// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using System.Linq;
using SVSModel.Configuration;
using SVSModel.Simulation;

namespace SVSModel.Models
{
    public class SoilWater
    {
        /// <summary>
        /// Calculates the soil water content and leaching risk daily leaching risk
        /// </summary>
        /// <param name="rswc">A date indexed dictionary of daily releative soil water content</param>
        /// <param name="drainage">a date indexed dictionary of daily water losses from the root zone</param>
        /// <param name="irrigation">A date indexed dictionary of assumed irrigation applied</param>
        /// <param name="meanRain"> A date indexed dictionary of long term mean daily rainfall</param>
        /// <param name="meanPET">A date indexed dictionary of long term meand daily potential evapo transpiration</param>
        /// <param name="cover">A date indexed dictionary of crop cover</param>
        /// <returns>Date indexed series of daily N mineralised from residues</returns>
        public static void Balance(ref SimulationType thisSim)
        {
            DateTime[] simDates = thisSim.simDates;
            Config config = thisSim.config;
            Dictionary<DateTime, double> SWC = Functions.dictMaker(simDates, new double[simDates.Length]);
            double dul = thisSim.config.Field.AWC;
            foreach (DateTime d in simDates)
            {
                if (d == simDates[0])
                {
                    SWC[simDates[0]] = dul * config.Field.PrePlantRainFactor;
                    thisSim.RSWC[simDates[0]] = SWC[simDates[0]] / dul;
                }
                else
                {
                    DateTime yest = d.AddDays(-1);
                    double T = Math.Min(SWC[yest] * 0.1, thisSim.meanPET[d] * thisSim.Cover[d]);
                    double E = thisSim.meanPET[d] * (1 - thisSim.Cover[d]) * thisSim.RSWC[yest];
                    SWC[d] = SWC[yest] + thisSim.meanRain[d] - T - E;
                    if (SWC[d] > dul)
                    {
                        thisSim.Drainage[d] = SWC[d] - dul;
                        SWC[d] = dul;
                    }
                    else
                    {
                        thisSim.Drainage[d] = 0.0;
                        if (SWC[d] / dul < config.Field.IrrigationTrigger)
                        {
                            double apply = dul * (config.Field.IrrigationRefill - config.Field.IrrigationTrigger);
                            SWC[d] += apply;
                            thisSim.Irrigation[d] = apply;
                        }
                    }
                }
                thisSim.RSWC[d] = SWC[d] / dul;
            }
        }
    }
}
