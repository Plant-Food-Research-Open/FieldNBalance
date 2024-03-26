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
        public List<CropConfig> Rotation { get; set; } = [];
        public FieldConfig Field { get; set; }

        /// <summary>
        /// Constructor used only by external webapp
        /// </summary>
        public Config() { }

        /// <summary>
        /// Constructor used only by the Excel model
        /// </summary>
        public Config(Dictionary<string, object> c)
        {
            // Only raw input values should be set in here
            
            Prior = new CropConfig(c, "Prior");
            Current = new CropConfig(c, "Current");
            Following = new CropConfig(c, "Following");
            Rotation = [Prior, Current, Following];
            Field = new FieldConfig(c);
        }
    }
}
