using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Analysis;
using SVSModel.Configuration;

namespace SVSModel
{
    public class Crop
    {
        /// <summary>
        /// Returns daily N uptake over the duration of the Tt input data for Root, Stover, Product and loss N as well as cover and root depth
        /// </summary>
        /// <param name="tt">An array containing the accumulated thermal time for the duration of the crop</param>
        /// <param name="cf">A specific class that holds all the simulation configuration data in the correct types for use in the model</param>
        /// <returns>A 2D array of crop model outputs</returns>
        public static object[,] Grow(Dictionary<DateTime, double> tt,
                                     CropConfig cf)
        {
            ///Set up data structures
            DateTime[] cropDates = Functions.DateSeries(cf.EstablishDate, cf.HarvestDate);
            DataFrame allCropParams = Crop.LoadCropCoefficients();
            CropParams cropParams = ExtractCropParams(cf.CropNameFull, allCropParams);// new Dictionary<string, double>();

            // Derive Crop Parameters
            double Tt_Harv = tt.Values.Last();
            double Tt_estab = Tt_Harv * (Constants.PropnTt[cf.EstablishStage] / Constants.PropnTt[cf.HarvestStage]);
            double Xo_Biomass = (Tt_Harv + Tt_estab) * .45 * (1 / Constants.PropnTt[cf.HarvestStage]);
            double b_Biomass = Xo_Biomass * .25;
            double T_mat = Xo_Biomass * 2.2222;
            double T_maxRD = Constants.PropnTt["EarlyReproductive"] * T_mat;
            double T_sen = Constants.PropnTt["MidReproductive"] * T_mat;
            double Xo_cov = Xo_Biomass * 0.4 / cropParams.rCover;
            double b_cov = Xo_cov * 0.2;
            double typicalYield = cropParams.TypicalYield * Constants.UnitConversions[cropParams.TypicalYieldUnits];
            double a_harvestIndex = cropParams.TypicalHI - cropParams.HIRange;
            double b_harvestIndex = cropParams.HIRange / typicalYield;
            double stageCorrection = 1 / Constants.PropnMaxDM[cf.HarvestStage];

            // derive crop Harvest State Variables 
            double fSaleableYieldFwt = cf.SaleableYield;
            double fFieldLossPct = cf.FieldLoss;
            double fTotalProductFwt = fSaleableYieldFwt * (1 / (1 - fFieldLossPct / 100)) * (1 / (1 - cf.DressingLoss / 100));
            // Crop Failure.  If yield is very low or field loss is very high assume complete crop failure.  Uptake equation are too sensitive saleable yields close to zero and field losses close to 100
            if ((cf.SaleableYield < (typicalYield * 0.05)) || (cf.FieldLoss > 95))
            {
                fFieldLossPct = 100;
                fTotalProductFwt = typicalYield * (1 / (1 - cropParams.TypicalDressingLoss / 100));
            }
            double fTotalProductDwt = fTotalProductFwt * (1 - cf.MoistureContent / 100);
            double fFieldLossDwt = fTotalProductDwt * fFieldLossPct / 100;
            double fFieldLossN = fFieldLossDwt * cropParams.ProductN / 100;
            double fDressingLossDwt = fTotalProductDwt * cf.DressingLoss / 100;
            double fDressingLossN = fDressingLossDwt * cropParams.ProductN / 100;
            double fSaleableProductDwt = fTotalProductDwt - fFieldLossDwt - fDressingLossDwt;
            double fSaleableProductN = fSaleableProductDwt * cropParams.ProductN / 100;
            double HI = a_harvestIndex + fTotalProductFwt * b_harvestIndex;
            double fStoverDwt = fTotalProductDwt * 1 / HI - fTotalProductDwt;
            double fStoverN = fStoverDwt * cropParams.StoverN / 100;
            double fRootDwt = (fStoverDwt + fTotalProductDwt) * cropParams.PRoot;
            double fRootN = fRootDwt * cropParams.RootN / 100;
            double fCropN = fRootN + fStoverN + fFieldLossN + fDressingLossN + fSaleableProductN;


            //Daily time-step, calculate Daily Scallers to give in-crop patterns
            Dictionary<DateTime, double> biomassScaller = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> coverScaller = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> rootDepthScaller = new Dictionary<DateTime, double>();
            foreach (DateTime d in tt.Keys)
            {
                double bmScaller = (1 / (1 + Math.Exp(-((tt[d] - Xo_Biomass) / (b_Biomass)))));
                biomassScaller.Add(d, bmScaller);
                double rdScaller = 1;
                if (tt[d] < T_maxRD)
                    rdScaller = tt[d] / T_maxRD;
                rootDepthScaller.Add(d, rdScaller);
                double cScaller = Math.Max(0, (1 - (tt[d] - T_sen) / (T_mat - T_sen)));
                if (tt[d] < T_sen)
                    cScaller = 1 / (1 + Math.Exp(-((tt[d] - Xo_cov) / b_cov)));
                coverScaller.Add(d, cScaller);
            }

            // Multiply Harvest State Variables by Daily Scallers to give Daily State Variables
            Dictionary<DateTime, double> RootN = Functions.scaledValues(biomassScaller, fRootN, stageCorrection);
            Dictionary<DateTime, double> StoverN = Functions.scaledValues(biomassScaller, fStoverN, stageCorrection);
            Dictionary<DateTime, double> SaleableProductN = Functions.scaledValues(biomassScaller, fSaleableProductN, stageCorrection);
            Dictionary<DateTime, double> FieldLossN = Functions.scaledValues(biomassScaller, fFieldLossN, stageCorrection);
            Dictionary<DateTime, double> DressingLossN = Functions.scaledValues(biomassScaller, fDressingLossN, stageCorrection);
            Dictionary<DateTime, double> TotalCropN = Functions.scaledValues(biomassScaller, fCropN, stageCorrection);
            Dictionary<DateTime, double> CropUptakeN = Functions.dictMaker(cropDates, Functions.calcDelta(TotalCropN.Values.ToArray()));
            Dictionary<DateTime, double> Cover = Functions.scaledValues(coverScaller, cropParams.Acover, 1.0);
            Dictionary<DateTime, double> RootDepth = Functions.scaledValues(rootDepthScaller, cropParams.MaxRD, 1.0);

            // Pack Daily State Variables into a 2D array so they can be output
            object[,] ret = new object[cropDates.Length + 1, 10];
            ret[0, 0] = "Date"; Functions.packRows(0, cropDates, ref ret);
            ret[0, 1] = "RootN"; Functions.packRows(1, RootN, ref ret);
            ret[0, 2] = "StoverN"; Functions.packRows(2, StoverN, ref ret);
            ret[0, 3] = "SaleableProductN"; Functions.packRows(3, SaleableProductN, ref ret);
            ret[0, 4] = "FieldLossN"; Functions.packRows(4, FieldLossN, ref ret);
            ret[0, 5] = "DressingLossN"; Functions.packRows(5, DressingLossN, ref ret);
            ret[0, 6] = "TotalCropN"; Functions.packRows(6, TotalCropN, ref ret);
            ret[0, 7] = "CropUptakeN"; Functions.packRows(7, CropUptakeN, ref ret);
            ret[0, 8] = "Cover"; Functions.packRows(8, Cover, ref ret);
            ret[0, 9] = "RootDepth"; Functions.packRows(9, RootDepth, ref ret);
            return ret;
        }

        public static DataFrame LoadCropCoefficients()
        {
            string resourceName = "SVSModel.Data.CropCoefficientTableFull.csv";
            var assembly = Assembly.GetExecutingAssembly();
            Stream csv = assembly.GetManifestResourceStream(resourceName);
            DataFrame allCropCoeffs = DataFrame.LoadCsv(csv);
            return allCropCoeffs;
        }

        public static CropParams ExtractCropParams(string crop, DataFrame allCropParams)
        {
            int cropRow = 0;
            bool cropNotFound = true;
            while (cropNotFound)
            {
                if (allCropParams[cropRow, 0].ToString() == crop)
                    cropNotFound = false;
                else
                    cropRow += 1;
            }

            List<string> coeffs = new List<string> { "Typical Yield","Typical Yield Units","Yield type","Typical Population (/ha)",
                                                      "TotalOrDry","Typical Dressing Loss %","Typical Field Loss %","Typical HI",
                                                      "HI Range","Moisture %","P Root","Max RD","A cover","rCover","Root [N]",
                                                      "Stover [N]","Product [N]" };

            Dictionary<string, object> cropParamDict = new Dictionary<string, object>();
            foreach (string c in coeffs)
            {
                cropParamDict.Add(c, allCropParams[c][cropRow]);
            }

            CropParams cropParams = new CropParams(cropParamDict);
            return cropParams;
        }
    }
}


