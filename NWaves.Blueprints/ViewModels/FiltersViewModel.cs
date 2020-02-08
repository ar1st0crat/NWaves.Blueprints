using Caliburn.Micro;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using System.Collections.Generic;
using System.Windows;

namespace NWaves.Blueprints.ViewModels
{
    public class FiltersViewModel : Screen
    {
        private static List<FilterNode> _filters;
        public List<FilterNode> Filters
        {
            get => _filters;
            set
            {
                _filters = value;
                NotifyOfPropertyChange(() => Filters);
            }
        }

        public FilterNode SelectedFilter { get; set; }

        public FiltersViewModel(IReflectionService reflectionService)
        {
            if (Filters == null)
            {
                Filters = reflectionService.BuildFiltersTree();
            }
        }

        public void OK()
        {
            if (SelectedFilter == null)
            {
                TryClose(false);
                return;
            }

            if (SelectedFilter.FilterType.IsAbstract)
            {
                MessageBox.Show($"The class {SelectedFilter.FilterType.FullName} is abstract!");
                return;
            }

            TryClose(true);
        }
    }
}
