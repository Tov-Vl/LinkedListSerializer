namespace SerializerTests.Interfaces
{
    public interface IListSerializerOptions
    {
        // The size of nodes collection (in bytes) after which serialization using System.Text.Json.JsonSerializer will be faster
        int CustomSerializationTotalBytesThreshold { get; set; }

        // The count of nodes in the nodes collection after which serialization using System.Text.Json.JsonSerializer will be faster
        int CustomSerializationNodesCountThreshold { get; set; }
    }
}
