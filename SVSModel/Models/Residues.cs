using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Data.Analysis;
using SVSModel.Configuration;

namespace SVSModel.Models
{
    public class Residues
    {
        /// <summary>
        /// Calculates the daily nitrogen mineralised as a result of residue decomposition
        /// </summary>
        /// <param name="meanT">A date indexed dictionary of daily mean temperatures</param>
        /// <param name="rswc">A date indexed dictionary of daily releative soil water content</param>
        /// <returns>Date indexed series of daily N mineralised from residues</returns>
        public static Dictionary<DateTime, double> Mineralisation(
            Dictionary<DateTime, double> rswc,
            Dictionary<DateTime, double> meanT,
            Config config)
        {
            DateTime[] simDates = rswc.Keys.ToArray();
            Dictionary<DateTime, double> NResidues = Functions.dictMaker(simDates, new double[simDates.Length]);

            ///Set up each cohort of residue
            DataFrame allCropParams = Crop.LoadCropCoefficients();
            CropParams priorCropParams = Crop.ExtractCropParams(config.Prior.CropNameFull, allCropParams);
            residue PresRoot = new residue(config.Prior.ResRoot, priorCropParams.RootN, config.Prior.HarvestDate, simDates, rswc, meanT);
            residue PresStover = new residue(config.Prior.ResStover, priorCropParams.StoverN, config.Prior.HarvestDate, simDates, rswc, meanT);
            residue PresFieldLoss = new residue(config.Prior.ResFieldLoss, priorCropParams.ProductN, config.Prior.HarvestDate, simDates, rswc, meanT);
            
            CropParams currentCropParams = Crop.ExtractCropParams(config.Current.CropNameFull, allCropParams);
            residue CresRoot = new residue(config.Current.ResRoot, currentCropParams.RootN, config.Current.HarvestDate, simDates, rswc, meanT);
            residue CresStover = new residue(config.Current.ResStover, currentCropParams.StoverN, config.Current.HarvestDate, simDates, rswc, meanT);
            residue CresFieldLoss = new residue(config.Current.ResFieldLoss, currentCropParams.ProductN, config.Current.HarvestDate, simDates, rswc, meanT);

            List<residue> Residues = new List<residue> { PresRoot, PresStover, PresFieldLoss, CresRoot, CresStover, CresFieldLoss };
            double[] TotalNetYesterday = new double[] {0,0,0,0,0,0};
            foreach (DateTime d in simDates)
            {
                double[] TotalNetToday = new double[6];
                int resInd = 0;
                foreach (residue r in Residues)
                {
                    TotalNetToday[resInd] += r.NetMineralisation[d];
                    resInd += 1;
                }
                NResidues[d] = TotalNetToday.Sum() - TotalNetYesterday.Sum();
                TotalNetYesterday = TotalNetToday;
            }
            
            return NResidues;
        }


    }

    public class residue
    {
        public static string name()
        {
            return typeof(residue).Name;
        }
        private double ANm { get; set; }
        private double ANi { get; set; } 
        private double Km { get; set; } 
        private double Ki { get; set; }
        
        public Dictionary<DateTime, double> NetMineralisation { get; set; }

        public residue(double amountN, double Nconc, DateTime additionDate, DateTime[] simDates, Dictionary<DateTime, double> rswc, Dictionary<DateTime, double> meanT)
        {
            double CNR = 40/Nconc;
            this.ANm = amountN * 0.8;
            this.ANi = amountN * (CNR * 2.5)/100;
            this.Km = 0.97 * Math.Exp(-0.12*CNR) + 0.03;
            this.Ki = 0.9 * Math.Exp(-0.12 * CNR) + 0.1; ;
            this.NetMineralisation = Functions.dictMaker(simDates, new double[simDates.Length]);
            double sigmaFtm = 0;
            foreach (DateTime d in simDates)
            {
                if (d >= additionDate)
                {
                    double Ft = SoilOrganic.LloydTaylorTemp(meanT[d]);
                    double Fm = SoilOrganic.QiuBeareCurtinWater(rswc[d]);
                    sigmaFtm += (Ft * Fm);
                    double mineralisation = ANm * (1 - Math.Exp(-Km * sigmaFtm));
                    double imobilisation = ANi * (1 - Math.Exp(-Ki * sigmaFtm));
                    NetMineralisation[d] = mineralisation - imobilisation;
                }
            }
        }

    }
}
