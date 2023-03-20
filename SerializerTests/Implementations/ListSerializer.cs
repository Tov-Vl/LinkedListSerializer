using SerializerTests.Interfaces;
using SerializerTests.Model;
using SerializerTests.Nodes;
using SerializerTests.Mappers;

namespace SerializerTests.Implementations
{
    public class ListSerializer : IListSerializer
    {

        //the constructor with no parameters is required and no other constructors can be used.
        public ListSerializer()
        {
            //...
        }

        public IMapper<ListNode, IEnumerable<NodeDto>> ListNodeToNodeDtosMapper { get; set; } = new ListNodeToNodeDtosMapper();

        public INodeDtosSerializer NodeDtosSerializer { get; set; } = new NodeDtosSerializer();

        public IMapper<IList<NodeDto>, ListNode> NodeDtosToListNodeMapper { get; set; } = new NodeDtosToListNodeMapper();

        public INodeDtosDeserializer NodeDtosDeserializer { get; set; } = new Utf8JsonReaderNodeDtosDeserializer();

        public Task<ListNode> DeepCopy(ListNode head)
        {
            var nodeDtos = ListNodeToNodeDtosMapper.Map(head);

            var headCopy = NodeDtosToListNodeMapper.Map(nodeDtos.ToList());

            return Task.FromResult(headCopy);
        }

        public async Task Serialize(ListNode head, Stream stream)
        {
            var nodeDtos = ListNodeToNodeDtosMapper.Map(head);

            await NodeDtosSerializer.Serialize(stream, nodeDtos);
        }

        public async Task<ListNode> Deserialize(Stream stream)
        {
            var nodeDtos = await NodeDtosDeserializer.Deserialize(stream);

            if (nodeDtos.Count == 0)
                throw new ArgumentException("No data in the stream");

            var head = NodeDtosToListNodeMapper.Map(nodeDtos);

            return head;
        }
    }
}