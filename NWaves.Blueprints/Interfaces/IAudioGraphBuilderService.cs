using NWaves.Blueprints.Models;
using NWaves.Filters.Base;
using System.Collections.Generic;

namespace NWaves.Blueprints.Interfaces
{
    public interface IAudioGraphBuilderService
    {
        IOnlineFilter Build(IEnumerable<FilterNode> nodes);
    }
}
