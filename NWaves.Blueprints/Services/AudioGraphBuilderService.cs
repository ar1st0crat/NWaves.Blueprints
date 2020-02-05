using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using NWaves.Filters.Base;

namespace NWaves.Blueprints.Services
{
    public class AudioGraphBuilderService : IAudioGraphBuilderService
    {
        public Func<float, float> Build(IEnumerable<FilterNode> nodes)
        {
            Func<float, float> chain = x => x;

            foreach (var node in nodes)
            {
                chain = Add(chain, CreateFilter(node));
            }

            return chain;
        }

        private static Func<float, float> Add(Func<float, float> func, IOnlineFilter filter)
        {
            return x => filter.Process(func(x));
        }

        private static IOnlineFilter CreateFilter(FilterNode node)
        {
            var ctor = node.FilterType.GetConstructors()[0];
            var paramTypes = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
            var parameters = new object[paramTypes.Length];

            for (var i = 0; i < node.Parameters.Length; i++)
            {
                if (typeof(IEnumerable<float>).IsAssignableFrom(paramTypes[i]))
                {
                    parameters[i] = node.Parameters[i]
                                     .ToString()
                                     .Split()
                                     .Select(e => float.Parse(e))
                                     .ToArray();
                }
                else
                {
                    parameters[i] = Convert.ChangeType(node.Parameters[i], paramTypes[i]);
                }
            }

            return (IOnlineFilter)ctor.Invoke(parameters);
        }
    }
}
