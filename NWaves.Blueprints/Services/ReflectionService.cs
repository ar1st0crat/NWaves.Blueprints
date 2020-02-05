using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using NWaves.Filters.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NWaves.Blueprints.Services
{
    public class ReflectionService : IReflectionService
    {
        private static readonly Assembly _nwaves;

        static ReflectionService()
        {
            _nwaves = Assembly.LoadFrom("NWaves.dll");
        }

        public List<FilterNode> BuildFiltersTree()
        {
            var filters = new List<FilterNode>();

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

                filters.Add(newNode);

                AddFilterNodes(tp, ref newNode);
            }

            return filters;
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

        public List<string> FilterParameters(Type type)
        {
            var info = type.GetConstructors()[0];

            return info.GetParameters()
                       .Select(p => p.Name)
                       .ToList();
        }
    }
}
