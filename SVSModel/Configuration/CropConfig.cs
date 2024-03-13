using System;
using System.Collections.Generic;
using SVSModel.Models;

namespace SVSModel.Configuration;

/// <summary>
/// Class that stores the configuration information in the correct type for a specific crop .  
/// I.e constructor takes all config settings as objects and converts them to appropriates types
/// </summary>
public class CropConfig
{
    public string CropNameFull { get; set; }
    public string EstablishStage { get; set; }
    public string HarvestStage { get; set; }

    public double _rawYield { private get; init; }
    public string _yieldUnits { private get; init; }
    public double? _population { private get; init; }
    public double FieldYield
    {
        get
        {
            if (_yieldUnits == "kg/head")
            {
                return _rawYield * _population.GetValueOrDefault();
            }

            var toKGperHA = Constants.UnitConversions[_yieldUnits ?? Defaults.Units];

            return _rawYield * toKGperHA;
        }
    }

    public double FieldLoss { get; set; }
    public double MoistureContent { get; set; }
    public DateTime EstablishDate { get; set; }
    public DateTime HarvestDate { get; set; }

    public string _residueRemoval { private get; init; }
    public double ResidueFactRetained => Constants.ResidueFactRetained[_residueRemoval];

    public string _residueIncorporation { private get; init; }
    public double ResidueFactIncorporated => Constants.ResidueIncorporation[_residueIncorporation];

    public double ResRoot { get; set; }
    public double ResStover { get; set; }
    public double ResFieldLoss { get; set; }
    public double NUptake { get; set; }

    /// <summary>
    /// Constructor used only by external webapp
    /// </summary>
    public CropConfig() { }

    /// <summary>
    /// Constructor used only by the Excel model
    /// </summary>
    public CropConfig(Dictionary<string, object> c, string pos)
    {
        CropNameFull = c[pos + "CropNameFull"].ToString();
        EstablishStage = c[pos + "EstablishStage"].ToString();
        HarvestStage = c[pos + "HarvestStage"].ToString();
        FieldLoss = Functions.Num(c[pos + "FieldLoss"]);
        MoistureContent = Functions.Num(c[pos + "MoistureContent"]);
        EstablishDate = Functions.Date(c[pos + "EstablishDate"]);
        HarvestDate = Functions.Date(c[pos + "HarvestDate"]);

        _residueRemoval = c[pos + "ResidueRemoval"].ToString();
        _residueIncorporation = c[pos + "ResidueIncorporation"].ToString();

        _rawYield = Functions.Num(c[pos + "SaleableYield"]);
        _yieldUnits = c[pos + "YieldUnits"].ToString();
        _population = Functions.Num(c[pos + "Population"]);
    }
}
