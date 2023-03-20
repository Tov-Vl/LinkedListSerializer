using SerializerTests.Model;

namespace SerializerTests.Interfaces
{
    public interface INodeDtosSerializer
    {
        Task Serialize(Stream stream, IEnumerable<NodeDto> nodes);
    }
}
