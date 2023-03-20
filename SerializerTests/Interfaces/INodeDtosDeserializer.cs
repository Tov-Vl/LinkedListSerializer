using SerializerTests.Model;

namespace SerializerTests.Interfaces
{
    public interface INodeDtosDeserializer
    {
        Task<IList<NodeDto>> Deserialize(Stream stream);
    }
}
