using SerializerTests.Interfaces;
using SerializerTests.Model;
using SerializerTests.Nodes;

namespace SerializerTests.Mappers
{
    public class NodeDtosToListNodeMapper : IMapper<IList<NodeDto>, ListNode>
    {
        public ListNode Map(IList<NodeDto> source)
        {
            var nodes = new ListNode[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                var nodeData = source[i];
                nodes[i] = new ListNode { Data = nodeData.Data };
            }

            for (int i = 0; i < source.Count; i++)
            {
                var nodeDto = source[i];
                var node = nodes[i];

                if (nodeDto.RandomIndex.HasValue)
                    node.Random = nodes[nodeDto.RandomIndex.Value];

                if (i > 0)
                {
                    node.Previous = nodes[i - 1];
                    nodes[i - 1].Next = node;
                }
            }

            return nodes.First();
        }
    }
}
