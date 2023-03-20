using SerializerTests.Interfaces;
using SerializerTests.Model;
using System.Buffers;
using System.Text.Json;

namespace SerializerTests.Implementations
{
    public class Utf8JsonReaderNodeDtosDeserializer : INodeDtosDeserializer
    {
        public Task<IList<NodeDto>> Deserialize(Stream stream)
        {
            IList<NodeDto> res = new List<NodeDto>();

            var readerOptions = new JsonReaderOptions
            {
                AllowTrailingCommas = true
            };

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            var reader = new Utf8JsonReader(buffer, readerOptions);

            if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Invalid JSON format (expected start of the array)", (stream as FileStream)?.Name, null, null);

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var node = new NodeDto();

                    while (reader.TokenType != JsonTokenType.EndObject)
                    {
                        reader.Read();

                        if (reader.TokenType == JsonTokenType.EndObject)
                            res.Add(node);
                        else if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            var propertyName = reader.GetString();
                            reader.Read();

                            switch (propertyName)
                            {
                                case NodeDto.IndexName:
                                    node.Index = reader.GetInt32();
                                    break;
                                case NodeDto.RandomIndexName:
                                    if (reader.TokenType == JsonTokenType.Null)
                                        node.RandomIndex = null;
                                    else
                                        node.RandomIndex = reader.GetInt32();
                                    break;
                                case NodeDto.DataName:
                                    if (reader.TokenType == JsonTokenType.Null)
                                        node.Data = null;
                                    else
                                        node.Data = reader.GetString();
                                    break;
                                default:
                                    throw new JsonException($"Wrong name of the property: \"{propertyName}\"", (stream as FileStream)?.Name, null, null);
                            }
                        }
                    }
                }
            }

            return Task.FromResult(res);
        }
    }
}
