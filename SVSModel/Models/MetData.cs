// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

namespace SVSModel.Models
{
    public class MetData
    {
        public int DoY { get; set; }
        public double MeanT { get; set; }
        public double Rain { get; set; }
        public double MeanPET { get; set; }

        public MetData() { }
    }
}
