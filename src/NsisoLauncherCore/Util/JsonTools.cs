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
                if (reader.Value == null)
                {
                    return null;
                }
                else
                {
                    return new Artifact((string)reader.Value);
                }
            }

            public override void WriteJson(JsonWriter writer, Artifact value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteValue((object)null);
                }
                else
                {
                    writer.WriteValue(value.Descriptor);
                }
            }
        }
    }
}
