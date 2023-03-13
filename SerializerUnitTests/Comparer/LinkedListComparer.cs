using SerializerTests.Nodes;

namespace SerializerUnitTests.Comparers
{
    internal class LinkedListComparer : IEqualityComparer<ListNode>
    {
        public bool Equals(ListNode? x, ListNode? y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;

            while (x != null && y != null)
            {
                if (!BothNullOrNotNull(x.Previous, y.Previous)
                    || !BothNullOrNotNull(x.Next, y.Next)
                    || !BothNullOrNotNull(x.Random, y.Random))
                    return false;

                if (x.Data != y.Data)
                    return false;

                x = x.Next;
                y = y.Next;
            }

            return true;

            static bool BothNullOrNotNull(ListNode? x, ListNode? y)
            {
                return x == null && y == null || x != null && y != null;
            }
        }

        public int GetHashCode(ListNode node)
        {
            unchecked
            {
                var hash = InnerGetHashCode(node);

                return hash;

                static int InnerGetHashCode(ListNode? node)
                {
                    if (node == null)
                        return 0;

                    unchecked
                    {
                        var hash = 17;
                        hash = hash * 23 + node.Data?.GetHashCode() ?? 0;

                        hash += InnerGetHashCode(node.Previous);
                        hash += InnerGetHashCode(node.Next);
                        hash += InnerGetHashCode(node.Random);

                        return hash;
                    }
                }
            }
        }
    }
}
