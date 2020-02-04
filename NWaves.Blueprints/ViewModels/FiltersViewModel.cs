using Caliburn.Micro;
using NWaves.Blueprints.Models;
using NWaves.Filters.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace NWaves.Blueprints.ViewModels
{
    public class FiltersViewModel : Screen
    {
        private static readonly Assembly _nwaves;

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


        static FiltersViewModel()
        {
            _nwaves = Assembly.LoadFrom("NWaves.dll");
        }

        public FiltersViewModel()
        {
            if (_filters == null)
            {
                BuildFiltersTree();
            }
        }

        private void BuildFiltersTree()
        {
            Filters = new List<FilterNode>();

            var type = typeof(IOnlineFilter);
            var types = _nwaves.GetTypes()
                               .Where(p => type.IsAssignableFrom(p) && p != type);

            foreach (var tp in types)
            {
                // at the first level add only "direct implementers" of IOnlineFilter:

                if (tp.BaseType.Name != "Object")
                {
                    continue;
                }

                var newNode = new FilterNode
                {
                    FilterType = tp
                };

                Filters.Add(newNode);

                AddFilterNodes(tp, ref newNode);
            }
        }

        private void AddFilterNodes(Type type, ref FilterNode node)
        {
            var types = _nwaves.GetTypes()
                               .Where(t => t.BaseType == type);

            node.Nodes = new List<FilterNode>();

            foreach (var tp in types)
            {
                var newNode = new FilterNode
                {
                    FilterType = tp
                };

                node.Nodes.Add(newNode);

                AddFilterNodes(tp, ref newNode);
            }
        }

        public void OK()
        {
            if (SelectedFilter.FilterType.IsAbstract)
            {
                MessageBox.Show($"The class {SelectedFilter.FilterType.FullName} is abstract!");
                return;
            }

            TryClose(true);
        }
    }
}
