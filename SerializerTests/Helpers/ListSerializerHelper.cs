using SerializerTests.Model;
using SerializerTests.Nodes;
using System.Text;
using System.Text.Json;

namespace SerializerTests.Helpers
{
    public static class ListSerializerHelper
    {
        public static async Task SerializeWithJsonSerializer(IDictionary<ListNode, NodeDto> nodeDtoDict, Stream stream, bool usePrettyFormatting = false)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = usePrettyFormatting
            };

            stream.WriteByte((byte)'[');
            foreach (var node in nodeDtoDict.Values)
            {
                await JsonSerializer.SerializeAsync(stream, node, options);
                stream.Write(",\n"u8);
            }
            stream.Position -= 2;
            stream.WriteByte((byte)']');
        }

        public static async Task Serialize(IDictionary<ListNode, NodeDto> nodeDtoDict, Stream stream, bool usePrettyFormatting = false)
        {
            var indent = usePrettyFormatting ? "  " : "";
            var newLine = usePrettyFormatting ? "\n" : "";
            var whiteSpace = usePrettyFormatting ? " " : "";

            var serializer = new StringBuilder();
            serializer.Append("[" + newLine);
            foreach (var nodeDto in nodeDtoDict.Values)
            {
                serializer.Append(indent + '{' + newLine);
                serializer.Append($"{indent}{indent}\"{NodeDto.IndexName}\":{whiteSpace}{nodeDto.Index},{newLine}");
                serializer.Append($"{indent}{indent}\"{NodeDto.RandomIndexName}\":{whiteSpace}{(nodeDto.RandomIndex.HasValue ? nodeDto.RandomIndex : "null")},{newLine}");
                serializer.Append($"{indent}{indent}\"{NodeDto.DataName}\":{whiteSpace}\"{nodeDto.Data ?? "null"}\"{newLine}");
                serializer.Append(indent + '}' + ',' + newLine);
            }
            serializer.Remove(serializer.Length - 2, 2);
            serializer.Append(newLine + "]");

            var bytes = Encoding.UTF8.GetBytes(serializer.ToString());
            await stream.WriteAsync(bytes);
        }
    }
}
