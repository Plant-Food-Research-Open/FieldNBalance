using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestModel
{
    public class TestConfig
    {

        public static  Dictionary<string, object> configDict = new Dictionary<string, object>
            {
                {"InitialN",50.0},
                {"SoilOrder","Brown"},
                {"SampleDepth","0-30cm"},
                {"BulkDensity",1.22},
                { "PMNtype","PMN"},
                {"PMN",60.0},
                { "Trigger",30.0},
                { "Efficiency",80.0},
                { "Splits",3.0},
                { "AWC",120.0},
                { "PrePlantRain","Typical"},
                { "InCropRain","Very Dry"},
                { "Irrigation","Full"},
                { "PriorCropNameFull","Barley Fodder General"},
                { "PriorSaleableYield",8.0},
                { "PriorFieldLoss",0.0},
                { "PriorDressingLoss",0.0},
                { "PriorMoistureContent",0.0},
                { "PriorEstablishDate",45153.0},
                { "PriorEstablishStage","Seed"},
                { "PriorHarvestDate",45183.0},
                { "PriorHarvestStage","EarlyReproductive"},
                { "PriorResidueRemoval","None removed"},
                { "PriorResidueIncorporation","Full (Plough)"},
                { "CurrentCropNameFull","Potato Vegetable General"},
                { "CurrentSaleableYield",64.0},
                { "CurrentFieldLoss",0.0},
                { "CurrentDressingLoss",0.0},
                { "CurrentMoistureContent",77.7},
                { "CurrentEstablishDate",45213.0},
                { "CurrentEstablishStage","Seed"},
                { "CurrentHarvestDate",45397.0},
                { "CurrentHarvestStage","LateReproductive"},
                { "CurrentResidueRemoval","None removed"},
                { "CurrentResidueIncorporation","Full (Plough)"},
                { "FollowingCropNameFull","Oat Fodder General"},
                { "FollowingSaleableYield",10.0},
                { "FollowingFieldLoss",0.0},
                { "FollowingDressingLoss",0.0},
                { "FollowingMoistureContent",0.0},
                { "FollowingEstablishDate",45412.0},
                { "FollowingEstablishStage","Seed"},
                { "FollowingHarvestDate",45532.0},
                { "FollowingHarvestStage","EarlyReproductive"},
                { "FollowingResidueRemoval","None removed"},
                { "FollowingResidueIncorporation","Full (Plough)"},
                //{ "WeatherStation","Ashburton"} 

            };
    }
}
