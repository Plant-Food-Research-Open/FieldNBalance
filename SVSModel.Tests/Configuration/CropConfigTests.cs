using System;
using System.Collections.Generic;
using SVSModel.Configuration;
using SVSModel.Models;
using Xunit;

namespace SVSModel.Tests.Configuration;

public class CropConfigTests
{
    private static readonly string CropNameFull = Defaults.CurrentCropNameFull;
    private static readonly string EstablishStage = Defaults.EstablishStage;
    private static readonly string HarvestStage = Defaults.HarvestStage;
    private static readonly double FieldLoss = Defaults.FieldLoss;
    private static readonly double MoistureContent = Defaults.MoistureContent;
    private static readonly DateTime EstablishDate = Defaults.EstablishDate;
    private static readonly DateTime HarvestDate = Defaults.HarvestDate;
    private static readonly string ResidueRemoval = Defaults.ResidueRemoval;
    private static readonly string ResidueIncorporation = Defaults.ResidueIncorporation;
    private static readonly double FieldYield = Defaults.FieldYield;
    private static readonly string YieldUnits = Defaults.Units;
    private static readonly double? Population = null;

    private readonly Dictionary<string, object> ExcelInputDict = new()
    {
        { "CurrentCropNameFull", CropNameFull },
        { "CurrentEstablishStage", EstablishStage },
        { "CurrentHarvestStage", HarvestStage },
        { "CurrentFieldLoss", FieldLoss },
        { "CurrentMoistureContent", MoistureContent },
        { "CurrentEstablishDate", EstablishDate },
        { "CurrentHarvestDate", HarvestDate },
        { "CurrentResidueRemoval", ResidueRemoval },
        { "CurrentResidueIncorporation", ResidueIncorporation },
        { "CurrentSaleableYield", FieldYield },
        { "CurrentYieldUnits", YieldUnits },
        { "CurrentPopulation", Population }
    };

    [Theory]
    [InlineData(10.0, "t/ha")]
    [InlineData(10_000.0, "kg/ha")]
    [InlineData(0.25, "kg/head", 25_000.0)]
    public void Test_CropConfig_Excel_Sets_Yield_Correctly(double yield, string units, double? population = null)
    {
        // Update values in the base dictionary
        ExcelInputDict["CurrentSaleableYield"] = yield;
        ExcelInputDict["CurrentYieldUnits"] = units;
        if (population.HasValue) ExcelInputDict["CurrentPopulation"] = population;
        
        var cropConfig = new CropConfig(ExcelInputDict, "Current");

        // Determine the expected yield
        var expectedFieldYield = yield * Constants.UnitConversions[units];
        if (population.HasValue) expectedFieldYield = yield * population.Value;
        
        Assert.Equal(cropConfig.FieldYield, expectedFieldYield);
    }

    [Fact]
    public void Test_CropConfig_Excel_Sets_Residues_Correctly()
    {
        var cropConfig = new CropConfig(ExcelInputDict, "Current");

        var expectedResidueFactRetained = Constants.ResidueFactRetained[ResidueRemoval];
        Assert.Equal(cropConfig.ResidueFactRetained, expectedResidueFactRetained);

        var expectedResidueFactIncorporated = Constants.ResidueIncorporation[ResidueIncorporation];
        Assert.Equal(cropConfig.ResidueFactIncorporated, expectedResidueFactIncorporated);
    }
    
    [Theory]
    [InlineData(10.0, "t/ha")]
    [InlineData(10_000.0, "kg/ha")]
    [InlineData(0.25, "kg/head", 25_000.0)]
    public void Test_CropConfig_WebApp_Sets_Yield_Correctly(double yield, string units, double? population = null)
    {
        var cropConfig = new CropConfig
        {
            CropNameFull = CropNameFull,
            EstablishStage = EstablishStage,
            HarvestStage = HarvestStage,
            FieldLoss = FieldLoss,
            MoistureContent = MoistureContent,
            EstablishDate = EstablishDate,
            HarvestDate = HarvestDate,
            _residueRemoval = ResidueRemoval,
            _residueIncorporation = ResidueIncorporation,
            _rawYield = yield,
            _yieldUnits = units,
            _population = population
        };

        // Determine the expected yield
        var expectedFieldYield = yield * Constants.UnitConversions[units];
        if (population.HasValue) expectedFieldYield = yield * population.Value;
        
        Assert.Equal(cropConfig.FieldYield, expectedFieldYield);
    }

    [Theory]
    [InlineData(10.0, "t/ha")]
    [InlineData(10_000.0, "kg/ha")]
    [InlineData(0.25, "kg/head", 25_000.0)]
    public void Test_CropConfig_Both_Constructors_Match(double yield, string units, double? population = null)
    {
        var cropConfig = new CropConfig
        {
            CropNameFull = CropNameFull,
            EstablishStage = EstablishStage,
            HarvestStage = HarvestStage,
            FieldLoss = FieldLoss,
            MoistureContent = MoistureContent,
            EstablishDate = EstablishDate,
            HarvestDate = HarvestDate,
            _residueRemoval = ResidueRemoval,
            _residueIncorporation = ResidueIncorporation,
            _rawYield = yield,
            _yieldUnits = units,
            _population = population
        };
        
        ExcelInputDict["CurrentSaleableYield"] = yield;
        ExcelInputDict["CurrentYieldUnits"] = units;
        if (population.HasValue) ExcelInputDict["CurrentPopulation"] = population;
        var cropConfigExcel = new CropConfig(ExcelInputDict, "Current");
        
        Assert.Equal(cropConfig.CropNameFull, cropConfigExcel.CropNameFull);
        Assert.Equal(cropConfig.EstablishStage, cropConfigExcel.EstablishStage);
        Assert.Equal(cropConfig.HarvestStage, cropConfigExcel.HarvestStage);
        Assert.Equal(cropConfig.FieldLoss, cropConfigExcel.FieldLoss);
        Assert.Equal(cropConfig.MoistureContent, cropConfigExcel.MoistureContent);
        Assert.Equal(cropConfig.EstablishDate, cropConfigExcel.EstablishDate);
        Assert.Equal(cropConfig.HarvestDate, cropConfigExcel.HarvestDate);
        Assert.Equal(cropConfig.FieldYield, cropConfigExcel.FieldYield);
        Assert.Equal(cropConfig.ResidueFactIncorporated, cropConfigExcel.ResidueFactIncorporated);
        Assert.Equal(cropConfig.ResidueFactRetained, cropConfigExcel.ResidueFactRetained);
    }
}
