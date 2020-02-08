using NWaves.Filters.Base;
using System;

namespace NWaves.Blueprints.Models
{
    public class CompositeFilter : IOnlineFilter
    {
        private readonly Func<float, float> _processFunc;

        public CompositeFilter(Func<float, float> processFunc) => _processFunc = processFunc;

        public float Process(float input) => _processFunc(input);

        public void Reset() { }
    }
}
