using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using System;

namespace NsisoLauncherCore.Util
{
    public class JsonTools
    {
        public class ArtifactJsonConverter : JsonConverter<Artifact>
        {
            public override Artifact ReadJson(JsonReader reader, Type objectType, Artifact existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return new Artifact((string)reader.Value);
            }

            public override void WriteJson(JsonWriter writer, Artifact value, JsonSerializer serializer)
            {
                writer.WriteValue(value.Descriptor);
            }
        }
    }
}
