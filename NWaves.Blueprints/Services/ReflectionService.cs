using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using NWaves.Effects;
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
                if (_nwaves.GetTypes().Count(t => t.BaseType == tp) > 0)
                {
                    continue;
                }

                var newNode = new FilterNode
                {
                    FilterType = tp
                };

                filters.Add(newNode);
            }

            return filters;
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
