using SerializerTests.Interfaces;
using SerializerTests.Model;
using System.Text;
using System.Text.Json;

namespace SerializerTests.Implementations
{
    public class NodeDtosDeserializer : INodeDtosDeserializer
    {
        public async Task<IList<NodeDto>> Deserialize(Stream stream)
        {
            var res = new List<NodeDto>();

            var node = new NodeDto();
            var buffer = new byte[1];
            var propertyIndex = 0;
            var depth = 0;
            var propertyValueBuilder = new StringBuilder();
            var propertyNameBuilder = new StringBuilder();

            while (await stream.ReadAsync(buffer.AsMemory(0, 1)) > 0)
            {
                if (depth > 1)
                    throw new JsonException("Wrong nesting depth of the JSON file", (stream as FileStream)?.Name, null, null);

                if (propertyIndex > NodeDto.PropertiesCount)
                    throw new JsonException($"Wrong number of object properties (expected {NodeDto.PropertiesCount} properties", (stream as FileStream)?.Name, null, null);

                var ch = (char)buffer[0];

                if (ch == '{')
                {
                    if (depth == 0)
                        node = new NodeDto();

                    depth++;
                }
                else if (depth > 0 && (ch == ',' || ch == '}'))
                {
                    propertyIndex++;

                    var propertyValue = propertyValueBuilder.ToString();
                    var propertyName = propertyNameBuilder.ToString();
                    switch (propertyName)
                    {
                        case NodeDto.IndexName:
                            node.Index = int.Parse(propertyValue);
                            break;
                        case NodeDto.RandomIndexName:
                            if (propertyValue == "null")
                                node.RandomIndex = null;
                            else
                                node.RandomIndex = int.Parse(propertyValue);
                            break;
                        case NodeDto.DataName:
                            if (propertyValue == "null")
                                node.Data = null;
                            else
                                node.Data = propertyValue;
                            break;
                        default:
                            throw new JsonException($"Wrong name of the property: \"{propertyName}\"", (stream as FileStream)?.Name, null, null);
                    }

                    propertyValueBuilder.Clear();
                    propertyNameBuilder.Clear();

                    if (ch == '}')
                    {
                        depth--;

                        if (depth == 0)
                        {
                            res.Add(node);
                            propertyIndex = 0;
                        }
                    }
                }
                else if (ch == '"')
                {
                    var builder = new StringBuilder();
                    while (await stream.ReadAsync(buffer.AsMemory(0, 1)) > 0)
                    {
                        var nextChar = (char)buffer[0];
                        if (nextChar == '"')
                            break;

                        builder.Append(nextChar);
                    }

                    if (propertyNameBuilder.Length == 0)
                        propertyNameBuilder = builder;
                    else
                        propertyValueBuilder = builder;
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    propertyValueBuilder.Append(ch);
                }
            }

            return res;
        }
    }
}
