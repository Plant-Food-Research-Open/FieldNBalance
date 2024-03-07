using System;
using System.Collections.Generic;
using System.Data;

namespace SVSModel.Configuration
{
    /// <summary>
    /// Class that stores all the configuration information for the simulation
    /// </summary>
    public class Config
    {
        public DateTime StartDate { get { return Prior.HarvestDate.AddDays(1); } }
        public DateTime EndDate { get { return Following.HarvestDate; } }
        public CropConfig Prior { get; set; }
        public CropConfig Current { get; set; }
        public CropConfig Following { get; set; }

        public List<CropConfig> Rotation { get { return new List<CropConfig> { Prior, Current, Following }; } } 

        public FieldConfig Field { get; set; }

        public Config() { }

        public Config(Dictionary<string, object> c)
        {
            Prior = new CropConfig(c, "Prior");
            Current = new CropConfig(c, "Current");
            Following = new CropConfig(c, "Following");
            Field = new FieldConfig(c);
        }
    }
}
