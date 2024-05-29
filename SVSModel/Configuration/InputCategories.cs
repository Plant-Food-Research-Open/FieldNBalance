// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System.Collections.Generic;
using System.ComponentModel;


namespace SVSModel.Configuration
{
    public static class InputCategories
    {
        public enum CropStage
        {
            Seed,
            Seedling,
            Vegetative,
            EarlyReproductive,
            MidReproductive,
            LateReproductive,
            Maturity,
            Late
        }

        public enum Units
        {
            [Description("t/ha")]
            tPerHa,
            [Description("kg/ha")]
            kgPerHa,
            [Description("kg/head")]
            kgPerHead
        }

        public enum ResidueTreatments
        {
            [Description("None removed")]
            NoneRemoved,
            [Description("Baled")]
            Baled,
            [Description("Burnt")]
            Burnt,
            [Description("Grazed")]
            Grazed,
            [Description("All removed")]
            AllRemoved
        }

        public enum ResidueIncorporations
        {
            [Description("None (Surface)")]
            None,
            [Description("Part (Cultivate)")]
            Part,
            [Description("Full (Plough)")]
            Full
        }

        public enum RelativeRain
        {
            [Description("Very Wet")]
            VeryWet,
            [Description("Wet")]
            Wet,
            [Description("Typical")]
            Typical,
            [Description("Dry")]
            Dry,
            [Description("Very Dry")]
            VeryDry
        }

        public enum IrrigationOptions 
        {
            None,
            Some,
            Full
        }


        public enum SoilCategoris
        {
            Sedimentary,
            Volcanic
        }

        public enum SampleDepths
        {
            [Description("0-15cm")]
            Top15cm,
            [Description("0-30cm")]
            Top30cm,
            [Description("0-60cm")]
            Top60cm,
            [Description("0-90cm")]
            Top90cm
        }

        public enum SoilTextures
        {
            [Description("Sand")]
            Sand,
            [Description("Loamy sand")]
            LoamySand,
            [Description("Sandy loam")]
            SandyLoam,
            [Description("Sandy clay")]
            SandyClay,
            [Description("Sandy clay loam")]
            SandyClayLoam,
            [Description("Loam")]
            Loam,
            [Description("Silt loam")]
            SiltLoam,
            [Description("Silt")]
            Silt,
            [Description("Silty clay loam")]
            SiltyClayLoam,
            [Description("Clay loam")]
            ClayLoam,
            [Description("Silty clay")]
            SiltyClay,
            [Description("Clay")]
            Clay
        }

        public enum TestType
        {
            QuickTest,
            LabTest
        }

        public enum TestMoisture
        {
            Dry,
            Moist,
            Wet
        }
    }
}
   