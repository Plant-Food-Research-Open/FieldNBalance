using System;
using System.Collections.Generic;

namespace SVSModel.Configuration
{
    /// <summary>
    /// Class that stores the configuration information in the correct type for a specific crop .  
    /// I.e constructor takes all config settings as objects and converts them to appropriates types
    /// </summary>
    public class CropConfig
    {
        public string CropNameFull { get; set; }
        public string EstablishStage { get; set; }
        public string HarvestStage { get; set; }

        /// <summary>
        /// Model code is expecting kg/ha, so this field _must_ be in those units
        /// </summary>
        public double SaleableYield { get; private set; }

        public double FieldLoss { get; set; }
        public double DressingLoss { get; set; }
        public double MoistureContent { get; set; }
        public DateTime EstablishDate { get; set; }
        public DateTime HarvestDate { get; set; }
        public double ResidueFactRetained { get; set; }
        public double ResidueFactIncorporated { get; set; }
        public double ResRoot { get; set; }
        public double ResStover { get; set; }
        public double ResFieldLoss { get; set; }
        public double NUptake { get; set; }

        public CropConfig() { }

        /// <summary>
        /// Constructor used only by the Excel model
        /// </summary>
        public CropConfig(Dictionary<string, object> c, string pos)
        {
            CropNameFull = c[pos + "CropNameFull"].ToString();
            EstablishStage = c[pos + "EstablishStage"].ToString();
            HarvestStage = c[pos + "HarvestStage"].ToString();
            // UI sends yield in t/ha but model works in kg/ha so convert here
            SaleableYield = Functions.Num(c[pos + "SaleableYield"]) * Constants.UnitConversions["t/ha"];
            FieldLoss = Functions.Num(c[pos + "FieldLoss"]);
            DressingLoss = Functions.Num(c[pos + "DressingLoss"]);
            MoistureContent = Functions.Num(c[pos + "MoistureContent"]);
            EstablishDate = Functions.Date(c[pos + "EstablishDate"]);
            HarvestDate = Functions.Date(c[pos + "HarvestDate"]);
            ResidueFactRetained = Constants.ResidueFactRetained[c[pos + "ResidueRemoval"].ToString()];
            ResidueFactIncorporated = Constants.ResidueIncorporation[c[pos + "ResidueIncorporation"].ToString()];
        }

        /// <summary>
        /// Call this after initializing Units
        /// Converts the raw value to kg/ha for the model code
        /// To be used by interfaces outside of the excel sheet
        /// </summary>
        /// <param name="rawYield">The raw value from form</param>
        /// <param name="rawUnits">The raw units from form</param>
        /// <param name="population">The amount of crop when measuring in kg/head</param>
        public void SetYield(double rawYield, string rawUnits, double? population)
        {
            var toKGperHA = Constants.UnitConversions[rawUnits];

            if (rawUnits == "kg/head")
            {
                SaleableYield = rawYield * population.GetValueOrDefault();
            }
            else
            {
                SaleableYield = rawYield * toKGperHA;
            }
        }
    }
}
