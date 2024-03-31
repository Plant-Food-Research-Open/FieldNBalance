// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Analysis;
using System.Reflection;
using SVSModel.Configuration;
using SVSModel.Simulation;

namespace SVSModel.Models
{
    public class Losses
    {
        private static int currentMonth { get; set; } =  0;
        private static bool initialised = false;
        private static DataFrame lossCoeffs;
        
        /// <summary>
        /// Calculates N lost on a day from soil mineralN and potential drainage
        /// </summary>
        /// <param name="d"></param>
        /// <param name="thisSim"></param>
        /// <returns></returns>
        public static double DailyLoss(DateTime d, SimulationType thisSim)
        {
            if (initialised == false) 
            {
                lossCoeffs = LoadLossCoefficients();
                initialised = true;
            }
            
            double b = 0.38;
            int month = d.Month;
            if (month != currentMonth)
                b = findLossCoefficient(month, thisSim.config.Field.Location);
            double PropDVol = thisSim.Drainage[d] / thisSim.config.Field.AWC;
            double PropNLoss = (PropDVol * b) / (1 + PropDVol * b);
            return PropNLoss * Math.Max(0, thisSim.SoilN[d]);
        }

        public static DataFrame LoadLossCoefficients()
        {
            string resourceName = "SVSModel.Data.LossCoefficientTable.csv";
            var assembly = Assembly.GetExecutingAssembly();
            Stream csv = assembly.GetManifestResourceStream(resourceName);
            DataFrame allLossCoeffs = DataFrame.LoadCsv(csv);
            return allLossCoeffs;
        }

        public static double findLossCoefficient(int month, string location)
        {
            for (int i = 0; i < lossCoeffs.Rows.Count; i++) 
            {
                if ((lossCoeffs[i, 0].ToString() == location) && (Functions.Num(lossCoeffs[i, 1]) == month))
                {
                    return Functions.Num(lossCoeffs[i, 2]);
                }
            }
            return 0.38;
        }

        

    }
}
