using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Server;
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

    public class LibraryJsonConverter : JsonConverter<Library>
    {
        public override Library ReadJson(JsonReader reader, Type objectType, Library existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            if (obj.ContainsKey("natives"))
            {
                Native native = new Native();
                serializer.Populate(obj.CreateReader(), native);
                return native;
            }
            else
            {
                //return obj.ToObject<Library>();
                Library library = new Library();
                serializer.Populate(obj.CreateReader(), library);
                return library;
            }
        }

        public override void WriteJson(JsonWriter writer, Library value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class VersionBaseJsonConverter : JsonConverter<VersionBase>
    {
        public override VersionBase ReadJson(JsonReader reader, Type objectType, VersionBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            if (obj.ContainsKey("arguments") && obj["arguments"].Type == JTokenType.Object)
            {
                VersionV2 v2 = new VersionV2();
                serializer.Populate(obj.CreateReader(), v2);
                return v2;
            }
            else if (obj.ContainsKey("minecraftArguments") && obj["minecraftArguments"].Type == JTokenType.String)
            {
                VersionV1 v1 = new VersionV1();
                serializer.Populate(obj.CreateReader(), v1);
                return v1;
            }
            else
            {
                throw new Exception("Unsupported version type");
            }
        }

        public override void WriteJson(JsonWriter writer, VersionBase value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class ServerDescriptionJsonConverter : JsonConverter<Chat>
    {
        public override Chat ReadJson(JsonReader reader, Type objectType, Chat existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return new Chat() { Text = reader.Value.ToString() };
            }
            else
            {
                JObject obj = JObject.Load(reader);
                Chat chat = new Chat();
                serializer.Populate(obj.CreateReader(), chat);
                return chat;
            }
        }

        public override void WriteJson(JsonWriter writer, Chat value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
