// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System.Collections.Generic;
using SVSModel.Simulation;

namespace SVSModel.Models
{
    public class DailyNBalanceDTO
    {
        public List<DailyNBalance> Results { get; set; } = [];
        public NBalanceSummary NBalance { get; set; }
    }
}