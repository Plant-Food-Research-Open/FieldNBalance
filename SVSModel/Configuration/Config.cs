using System.Collections.Generic;

namespace SVSModel.Configuration
{
    /// <summary>
    /// Class that stores all the configuration information for the simulation
    /// </summary>
    public class Config
    {
        public CropConfig Prior { get; set; }
        public CropConfig Current { get; set; }
        public CropConfig Following { get; set; }

        public List<CropConfig> Rotation = new List<CropConfig>();

        public FieldConfig Field { get; set; }

        public Config() { }

        public Config(Dictionary<string, object> c)
        {
            Prior = new CropConfig(c, "Prior");
            Current = new CropConfig(c, "Current");
            Following = new CropConfig(c, "Following");
            Rotation = new List<CropConfig> { Prior, Current, Following };
            Field = new FieldConfig(c);
        }
    }
}
