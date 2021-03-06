﻿using System;
using System.Collections.Generic;

namespace NWaves.Blueprints.Models
{
    public class FilterNode
    {
        public Type FilterType { get; set; }
        public List<FilterNode> Nodes { get; set; }
        public FilterParameter[] Parameters { get; set; }
    }
}
