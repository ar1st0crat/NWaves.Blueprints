using Caliburn.Micro;
using NetworkModel;

namespace NWaves.Blueprints.ViewModels
{
    public class FilterNodeViewModel
    {
        public NodeViewModel NetworkNode { get; set; }
        public BindableCollection<ParameterViewModel> Parameters { get; set; }
    }
}
