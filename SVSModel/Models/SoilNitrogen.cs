using System;
using System.Collections.Generic;
using System.Linq;
using SVSModel.Configuration;

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
            Dictionary<DateTime, double> som,
            Config config)
        {
            DateTime[] simDates = uptake.Keys.ToArray();
            Dictionary<DateTime, double> soilN = Functions.dictMaker(simDates, new double[simDates.Length]);
            foreach (DateTime d in simDates)
            {
                if (d == simDates[0])
                {
                    soilN[simDates[0]] = config.Field.InitialN;
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
        /// Takes soil mineral N test values and adjustes to predicted N balance to correspond with these values on their specific dates
        /// </summary>
        /// <param name="testResults">date indexed series of test results</param>
        /// <param name="soilN">date indexed series of soil mineral N estimates to be corrected with measurements.  Passed in as ref so 
        /// the corrections are applied to the property passed in</param>
        public static void TestCorrection(
            Dictionary<DateTime, double> testResults,
            ref Dictionary<DateTime, double> soilN)
        {
            foreach (DateTime d in testResults.Keys)
            {
                double correction = testResults[d] - soilN[d];
                DateTime[] simDatesToCorrect = Functions.DateSeries(d, soilN.Keys.Last());
                foreach (DateTime c in simDatesToCorrect)
                {
                    soilN[c] += correction;
                }
            }
        }
    }
}
