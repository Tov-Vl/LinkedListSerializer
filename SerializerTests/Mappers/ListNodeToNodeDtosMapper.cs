using SerializerTests.Interfaces;
using SerializerTests.Model;
using SerializerTests.Nodes;

namespace SerializerTests.Mappers
{
    public class ListNodeToNodeDtosMapper : IMapper<ListNode, IEnumerable<NodeDto>>
    {
        public IEnumerable<NodeDto> Map(ListNode head)
        {
            var res = new Dictionary<ListNode, NodeDto>();

            int index = 0;
            for (var node = head; node != null; node = node.Next)
                res[node] = new NodeDto { Index = index++, Data = node.Data };

            foreach (var (node, data) in res)
                data.RandomIndex = node.Random == null ? null : res[node.Random].Index;

            return res.Values;
        }
    }
}
