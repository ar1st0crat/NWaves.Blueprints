using NWaves.Blueprints.Models;
using System.Collections.Generic;

namespace NWaves.Blueprints.Interfaces
{
    public interface ISerializationService
    {
        void Serialize(string filename, IEnumerable<FilterNode> filters);
        List<FilterNode> Deserialize(string filename);
    }
}
