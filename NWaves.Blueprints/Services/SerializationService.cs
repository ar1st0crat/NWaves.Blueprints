using Newtonsoft.Json;
using NWaves.Blueprints.Interfaces;
using NWaves.Blueprints.Models;
using System.Collections.Generic;
using System.IO;

namespace NWaves.Blueprints.Services
{
    public class SerializationService : ISerializationService
    {
        public List<FilterNode> Deserialize(string filename)
        {
            using (var f = File.OpenText(filename))
            {
                var json = f.ReadToEnd();

                return JsonConvert.DeserializeObject<List<FilterNode>>(json,
                            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
            }
        }

        public void Serialize(string filename, IEnumerable<FilterNode> filters)
        {
            var json = JsonConvert.SerializeObject(filters, Formatting.Indented,
                            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            using (var f = File.CreateText(filename))
            {
                f.Write(json);
            }
        }
    }
}
