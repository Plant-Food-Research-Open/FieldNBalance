using System.Collections.Generic;

namespace SVSModel.Configuration
{
    /// <summary>
    /// Class that stores the configuration information for a specific crop in the correct type.  
    /// I.e constructor takes all config settings as objects and converts them to appropriates types
    /// </summary>
    public class CropParams
    {
        public double TypicalYield { get; private set; }
        public double TypicalYield_kgPerHa
        {
            get
            {
                if (TypicalYieldUnits == "kg/head") return TypicalYield * TypicalPopulation;

                return TypicalYield * Constants.UnitConversions[TypicalYieldUnits];
            }
        }
        public string TypicalYieldUnits { get; private set; }
        public string YieldType { get; private set; }
        public double TypicalPopulation { get; private set; }
        public string TotalOrDry { get; private set; }
        public double TypicalDressingLoss { get; private set; }
        public double TypicalFieldLoss { get; private set; }
        public double TypicalHI { get; private set; }
        public double HIRange { get; private set; }
        public double Moisture { get; private set; }
        public double PRoot { get; private set; }
        public double MaxRD { get; private set; }
        public double Acover { get; private set; }
        public double rCover { get; private set; }
        public double RootN { get; private set; }
        public double StoverN { get; private set; }
        public double ProductN { get; private set; }

        public CropParams(Dictionary<string, object> c)
        {
            TypicalYield = Functions.Num(c["Typical Yield"]);
            TypicalYieldUnits = c["Typical Yield Units"].ToString();
            YieldType = c["Yield type"].ToString();
            TypicalPopulation = Functions.Num(c["Typical Population (/ha)"]);
            TotalOrDry = c["TotalOrDry"].ToString();
            TypicalDressingLoss = Functions.Num(c["Typical Dressing Loss %"]);
            TypicalFieldLoss = Functions.Num(c["Typical Field Loss %"]);
            TypicalHI = Functions.Num(c["Typical HI"]);
            HIRange = Functions.Num(c["HI Range"]);
            Moisture = Functions.Num(c["Moisture %"]);
            PRoot = Functions.Num(c["P Root"]);
            MaxRD = Functions.Num(c["Max RD"]);
            Acover = Functions.Num(c["A cover"]);
            rCover = Functions.Num(c["rCover"]);
            RootN = Functions.Num(c["Root [N]"]);
            StoverN = Functions.Num(c["Stover [N]"]);
            ProductN = Functions.Num(c["Product [N]"]);
        }
    }
}
