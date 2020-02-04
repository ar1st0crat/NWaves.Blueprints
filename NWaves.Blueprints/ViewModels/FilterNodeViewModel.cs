using NetworkModel;
using System;
using System.Collections.ObjectModel;

namespace NWaves.Blueprints.ViewModels
{
    public class FilterNodeViewModel
    {
        public Type FilterType { get; set; }
        //public ObservableCollection<FilterNodeViewModel> Nodes { get; set; }
        public ObservableCollection<ParameterViewModel> Parameters { get; set; }
        public NodeViewModel NetworkNode { get; set; }
    }
}
