using NWaves.Blueprints.Models;
using System;
using System.Collections.Generic;

namespace NWaves.Blueprints.Interfaces
{
    public interface IReflectionService
    {
        List<FilterNode> BuildFiltersTree();
        List<string> FilterParameters(Type type);
    }
}
