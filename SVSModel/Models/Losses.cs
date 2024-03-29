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
    public class Losses
    {
        /// <summary>
        /// Calculates N lost on a day from soil mineralN and potential drainage
        /// </summary>
        /// <param name="d"></param>
        /// <param name="thisSim"></param>
        /// <returns></returns>
        public static double DailyLoss(DateTime d, SimulationType thisSim)
        {
            double b = 0.38;
            double PropDVol = thisSim.Drainage[d] / thisSim.config.Field.AWC;
            double PropNLoss = (PropDVol * b) / (1 + PropDVol * b);
            return PropNLoss * Math.Max(0, thisSim.SoilN[d]);
        }

    }
}
