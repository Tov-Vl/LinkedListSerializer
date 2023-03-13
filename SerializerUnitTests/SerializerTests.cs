using SerializerTests.Helpers;
using SerializerTests.Implementations;
using SerializerTests.Interfaces;
using SerializerTests.Mappers;
using SerializerTests.Model;
using SerializerTests.Nodes;
using SerializerTests.Options;
using SerializerUnitTests.Comparers;
using System.Diagnostics;
using System.Text;

namespace SerializerUnitTests
{
    [TestFixture]
    public class SerializerTests
    {
        private string _filePath;
        private IListSerializerOptions _options;

        [SetUp]
        public void Setup()
        {
            var fileDirectory = Environment.CurrentDirectory;
            var fileName = "serialized_linked_list.txt";

            _filePath = Path.Combine(fileDirectory, fileName);
            _options = new ListSerializerOptions();
        }

        [Test, Order(1)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(10000)]
        public void Serialize_LinkedList_DoesNotThrowException(int nodesCount)
        {
            // Arrange
            var head = GenerateLinkedList(nodesCount);
            var linkedListSerializer = new ListSerializer();
            using var stream = File.Create(_filePath);

            // Act
            async Task serializeAsync() => await linkedListSerializer.Serialize(head, stream);

            // Assert
            Assert.DoesNotThrowAsync(serializeAsync);
        }

        [Test, Order(2)]
        public void Deserialize_StreamWithLinkedList_DoesNotThrowException()
        {
            // Arrange
            var linkedListSerializer = new ListSerializer();
            using var stream = File.OpenRead(_filePath);

            // Act
            async Task deserializeAsync() => _ = await linkedListSerializer.Deserialize(stream);

            // Assert
            Assert.DoesNotThrowAsync(deserializeAsync);
        }

        [TestCase(1)]
        [TestCase(10)]
        public async Task Deserialize_SerializedLinkedList_ReturnsSameList(int nodesCount)
        {
            // Arrange
            var linkedList = GenerateLinkedList(nodesCount);
            var linkedListSerializer = new ListSerializer();
            var linkedListComparer = new LinkedListComparer();

            // Act
            using (var stream = File.Create(_filePath))
            {
                await linkedListSerializer.Serialize(linkedList, stream);
            }

            ListNode? linkedListDeserialized;
            using (var stream = File.OpenRead(_filePath))
            {
                linkedListDeserialized = await linkedListSerializer.Deserialize(stream);
            }

            // Assert
            Assert.That(linkedList, Is.EqualTo(linkedListDeserialized).Using(linkedListComparer));
        }

        [TestCase(1)]
        [TestCase(10)]
        public async Task DeepCopy_LinkedList_ReturnsSameList(int nodesCount)
        {
            // Arrange
            var linkedList = GenerateLinkedList(nodesCount);
            var linkedListDeserialized = new ListSerializer();
            var linkedListComparer = new LinkedListComparer();

            // Act
            var linkedListDeepCopy = await linkedListDeserialized.DeepCopy(linkedList);

            // Assert
            Assert.That(linkedList, Is.EqualTo(linkedListDeepCopy).Using(linkedListComparer));
        }

        [TestCase(1)]
        [TestCase(10)]
        public async Task DeepCopy_LinkedList_ReferencesAreNotEqual(int nodesCount)
        {
            // Arrange
            var linkedList = GenerateLinkedList(nodesCount);
            var linkedListDeserialized = new ListSerializer();

            // Act
            var linkedListDeepCopy = await linkedListDeserialized.DeepCopy(linkedList);

            // Assert
            Assert.That(NodesReferencesAreNotEqual(linkedList, linkedListDeepCopy), Is.True);
        }

        [TestCase(1, 0.5)]
        [TestCase(0.7, 0.5)]
        [TestCase(0.01, 0.005)]
        public async Task HelperSerialize_LinkedList_FasterThanJsonSerializer(double fractionOfTheThreshold, double fractionOfNodesCountThreshold)
        {
            // Arrange
            var stopWatch = new Stopwatch();
            var nodesCount = (int)Math.Ceiling(_options.CustomSerializationNodesCountThreshold * fractionOfNodesCountThreshold);
            var nodeDtoSize = _options.CustomSerializationTotalBytesThreshold * fractionOfTheThreshold / nodesCount;
            var head = GenerateLinkedList(nodesCount, nodeDtoSize);
            var listNodeToNodeDtosMapper = new ListNodeToNodeDtosMapper();
            var nodeDtoDict = listNodeToNodeDtosMapper.Map(head);

            // Act
            stopWatch.Start();
            using (var stream = File.Create(_filePath))
                await ListSerializerHelper.Serialize(nodeDtoDict, stream, usePrettyFormatting: true);
            var customSerializationTime = stopWatch.Elapsed;

            stopWatch.Restart();
            using (var stream = File.Create(_filePath))
                await ListSerializerHelper.SerializeWithJsonSerializer(nodeDtoDict, stream, usePrettyFormatting: true);
            var systemTextJsonSerializationTime = stopWatch.Elapsed;

            var message = $"""
                            Custom serialization time: {customSerializationTime.TotalMilliseconds:0} ms
                            System.Test.Json serialization time: {systemTextJsonSerializationTime.TotalMilliseconds:0} ms
                            """;
            Console.WriteLine(message);

            // Assert
            Assert.That(customSerializationTime, Is.LessThan(systemTextJsonSerializationTime));
        }

        [TestCase(1, 1.1)]
        [TestCase(1.1, 1)]
        [TestCase(1.1, 1.1)]
        [TestCase(2, 2)]
        public async Task HelperSerialize_LinkedList_SlowerThanJsonSerializer(double fractionOfTotalBytesThreshold, double fractionOfNodesCountThreshold)
        {
            // Arrange
            var stopWatch = new Stopwatch();
            var nodesCount = (int) Math.Ceiling(_options.CustomSerializationNodesCountThreshold * fractionOfNodesCountThreshold);
            var nodeDtoSize = _options.CustomSerializationTotalBytesThreshold * fractionOfTotalBytesThreshold / nodesCount;
            var head = GenerateLinkedList(nodesCount, nodeDtoSize);
            var listNodeToNodeDtosMapper = new ListNodeToNodeDtosMapper();
            var nodeDtoDict = listNodeToNodeDtosMapper.Map(head);

            // Act
            stopWatch.Start();
            using (var stream = File.Create(_filePath))
                await ListSerializerHelper.Serialize(nodeDtoDict, stream, usePrettyFormatting: true);
            var customSerializationTime = stopWatch.Elapsed;

            stopWatch.Restart();
            using (var stream = File.Create(_filePath))
                await ListSerializerHelper.SerializeWithJsonSerializer(nodeDtoDict, stream, usePrettyFormatting: true);
            var systemTextJsonSerializationTime = stopWatch.Elapsed;

            var message = $"""
                            Custom serialization time: {customSerializationTime.TotalMilliseconds:0} ms
                            System.Test.Json serialization time: {systemTextJsonSerializationTime.TotalMilliseconds:0} ms
                            """;
            Console.WriteLine(message);

            // Assert
            Assert.That(customSerializationTime, Is.GreaterThan(systemTextJsonSerializationTime));
        }

        private static ListNode GenerateLinkedList(int nodesCount, double? nodeDtoSize = null)
        {
            if (nodesCount == 0)
                throw new ArgumentException("Nodes count should be greater than zero!", nameof(nodesCount));

            var nodes = new ListNode[nodesCount];
            var randGen = new Random();

            int dataLength;
            if (nodeDtoSize == null)
                dataLength = 10;
            else
                dataLength = GetDataLength((int)Math.Ceiling(nodeDtoSize.Value));

            if (dataLength <= 0)
                throw new ArgumentException($"The size of a single node is too small! Cant fit nodes with size of {nodeDtoSize:0.#} bytes into array with {nodesCount} elements", nameof(nodeDtoSize));

            for (var i = 0; i < nodesCount; i++)
            {
                var data = GetRandomString(dataLength);
                nodes[i] = new ListNode() { Data = data };
            }

            for (var i = 0; i < nodesCount; i++)
            {
                var node = nodes[i];

                if (randGen.Next(2) == 1)
                    node.Random = nodes[randGen.Next(0, nodesCount)];

                if (i > 0)
                {
                    node.Previous = nodes[i - 1];
                    nodes[i - 1].Next = node;
                }
            }

            return nodes.First();

            int GetDataLength(int nodeDtoSize)
            {
                return (nodeDtoSize - NodeDto.SizeOfIndex() - NodeDto.SizeOfRandomIndex()) / sizeof(char);
            }

            string? GetRandomString(int length)
            {
                if (randGen.Next(0, 2) == 0)
                    return null;

                var bulider = new StringBuilder();
                var counter = 0;
                while (counter < length)
                {
                    bulider.Append((char)randGen.Next(65, 91));
                    counter++;
                }

                return bulider.ToString();
            }
        }

        private static bool NodesReferencesAreNotEqual(ListNode? x, ListNode? y)
        {
            while (x != null && y != null)
            {
                if (ReferenceEquals(x, y))
                    return false;

                x = x.Next;
                y = y.Next;
            }

            return true;
        }
    }
}