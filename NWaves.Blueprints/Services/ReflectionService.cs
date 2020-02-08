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

        public FilterParameter[] GetFilterParameters(Type type)
        {
            // parameters are taken from:

            // 1) constructor (currently, just take the first constructor)
             
            var info = type.GetConstructors()[0];

            var pars = info.GetParameters()
                           .Select(p => new FilterParameter
                           {
                               Name = p.Name,
                               Type = p.ParameterType,
                               Value = p.RawDefaultValue
                           });
            
            // 2) properties Wet and Dry (if they exist)

            var props = type.GetProperties()
                            .Where(p => p.Name == "Wet" || p.Name == "Dry")
                            .Select(p => new FilterParameter
                            {
                                Name = p.Name,
                                Type = p.PropertyType,
                                Value = p.Name == "Wet" ? 1 : 0
                            });

            return pars.Concat(props).ToArray();
        }
    }
}
