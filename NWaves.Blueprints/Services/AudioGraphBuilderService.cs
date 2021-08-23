using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using NWaves.Effects;
using NWaves.Filters.Base;

namespace NWaves.Blueprints.Services
{
    public class AudioGraphBuilderService : IAudioGraphBuilderService
    {
        /// <summary>
        /// Building filter for entire audio graph involves following steps:
        /// 
        /// 1) build chains of connected filters
        /// 2) add all chains
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public IOnlineFilter Build(IEnumerable<FilterNode> nodes)
        {
            var nodeArray = nodes.ToArray();

            // find chains of filters in the entire network:
            
            var chains = new List<List<FilterNode>>();

            HashSet<int> indices = new HashSet<int>();

            var nodeCount = nodeArray.Length;

            while (indices.Count < nodeCount)
            {
                var chain = new List<FilterNode>();

                var i = 0;
                for (; i < nodeCount; i++)
                {
                    if (!indices.Contains(i)) break;
                }

                var node = nodeArray[i];

                var nextNode = node;
                while (nextNode.Nodes != null)
                {
                    nextNode = nextNode.Nodes[0];
                    chain.Add(nextNode);
                    indices.Add(Array.IndexOf(nodeArray, nextNode));
                }

                var prevNode = node;
                while (prevNode != null)
                {
                    chain.Add(prevNode);
                    indices.Add(Array.IndexOf(nodeArray, prevNode));
                    prevNode = nodes.FirstOrDefault(n => n.Nodes != null && n.Nodes[0] == prevNode);
                }

                chains.Add(chain);
            }

            // compose chains into one filter:

            if (chains.Any())
            {
                var funcs = chains.Select(filters => BuildChain(filters)).ToArray();

                return new CompositeFilter(x => funcs.Sum(f => f(x)));
            }
            else
            {
                return new CompositeFilter(x => x);
            }
        }

        private static Func<float, float> BuildChain(IEnumerable<FilterNode> nodes)
        {
            Func<float, float> chainFunc = null;

            foreach (var node in nodes)
            {
                chainFunc = AddFilterToChain(chainFunc, CreateFilter(node));
            }

            return chainFunc;
        }

        private static Func<float, float> AddFilterToChain(Func<float, float> func, IOnlineFilter filter)
        {
            if (func == null)
            {
                return x => filter.Process(x);
            }
            else
            {
                return x => filter.Process(func(x));
            }
        }

        private static IOnlineFilter CreateFilter(FilterNode node)
        {
            var parameters = 
                node.Parameters
                    .Where(p => p.Name != "Wet" && p.Name != "Dry")
                    .Select(parameter =>
                    {
                        if (typeof(IEnumerable<float>).IsAssignableFrom(parameter.Type))
                        {
                            return parameter.Value
                                            .ToString()
                                            .Split()
                                            .Select(e => float.Parse(e, CultureInfo.InvariantCulture))
                                            .ToArray();
                        }
                        else if (typeof(Enum).IsAssignableFrom(parameter.Type))
                        {
                            return Enum.Parse(parameter.Type, parameter.Value.ToString());
                        }
                        else
                        {
                            return Convert.ChangeType(parameter.Value, parameter.Type, CultureInfo.InvariantCulture);
                        }
                    });

            var ctor = node.FilterType.GetConstructors()[0];

            if (node.FilterType == typeof(VibratoEffect) ||
                node.FilterType == typeof(FlangerEffect))     // ver.0.9.5 upd.
            {
                ctor = node.FilterType.GetConstructors()[1];
            }

            var filter = (IOnlineFilter)ctor.Invoke(parameters.ToArray());


            // properties Wet and Dry:

            if (node.Parameters.Any(p => p.Name == "Wet"))
            {
                var propWet = node.FilterType.GetProperty("Wet");
                var propDry = node.FilterType.GetProperty("Dry");

                var valueWet = node.Parameters.First(p => p.Name == "Wet").Value;
                var valueDry = node.Parameters.First(p => p.Name == "Dry").Value;

                propWet.SetValue(filter, Convert.ChangeType(valueWet, typeof(float), CultureInfo.InvariantCulture));
                propDry.SetValue(filter, Convert.ChangeType(valueDry, typeof(float), CultureInfo.InvariantCulture));
            }

            return filter;
        }
    }
}
