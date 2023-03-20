using SerializerTests.Interfaces;
using SerializerTests.Model;
using System.Text.Json;

namespace SerializerTests.Implementations
{
    public class NodeDtosSerializer : INodeDtosSerializer
    {
        public async Task Serialize(Stream stream, IEnumerable<NodeDto> nodes)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            stream.WriteByte((byte)'[');
            foreach (var node in nodes)
            {
                await JsonSerializer.SerializeAsync(stream, node, options);
                stream.Write(",\n"u8);
            }
            stream.Position -= 2;
            stream.WriteByte((byte)']');
        }
    }
}
