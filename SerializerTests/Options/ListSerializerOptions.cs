using SerializerTests.Interfaces;

namespace SerializerTests.Options
{
    public class ListSerializerOptions: IListSerializerOptions
    {
        // The size of nodes collection (in bytes) after which serialization using System.Text.Json.JsonSerializer will be faster
        public int CustomSerializationTotalBytesThreshold { get; set; } = 5_500_000;

        // The count of nodes in the nodes collection after which serialization using System.Text.Json.JsonSerializer will be faster
        public int CustomSerializationNodesCountThreshold { get; set; } = 550_000;
    }
}
