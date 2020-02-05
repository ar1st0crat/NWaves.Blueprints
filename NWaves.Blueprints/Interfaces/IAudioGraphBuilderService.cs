using NWaves.Blueprints.Models;
using System;
using System.Collections.Generic;

namespace NWaves.Blueprints.Interfaces
{
    public interface IAudioGraphBuilderService
    {
        Func<float, float> Build(IEnumerable<FilterNode> nodes);
    }
}
