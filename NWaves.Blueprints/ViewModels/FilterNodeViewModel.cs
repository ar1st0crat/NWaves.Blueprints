using NetworkModel;
using System;
using System.Collections.Generic;

namespace NWaves.Blueprints.ViewModels
{
    public class FilterNodeViewModel
    {
        public Type FilterType { get; set; }
        public List<ParameterViewModel> Parameters { get; set; }
        public NodeViewModel NetworkNode { get; set; }
    }
}
