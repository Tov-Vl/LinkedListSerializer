using SerializerTests.Implementations;
using SerializerTests.Nodes;
using SerializerUnitTests.Comparers;
using System.Diagnostics;
using System.Text;

namespace SerializerUnitTests
{
    [TestFixture]
    public class SerializerTests
    {
        private string _filePath;

        [SetUp]
        public void Setup()
        {
            var fileDirectory = Environment.CurrentDirectory;
            var fileName = "serialized_linked_list.json";

            _filePath = Path.Combine(fileDirectory, fileName);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            File.Delete(_filePath);
        }

        [Test, Order(1)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(10_000)]
        [TestCase(600_000)]
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
        public void Deserialize_StreamWithLinkedListAndCustomDeserializer_DoesNotThrowException()
        {
            // Arrange
            var linkedListSerializer = new ListSerializer()
            {
                NodeDtosDeserializer = new NodeDtosDeserializer()
            };
            using var stream = File.OpenRead(_filePath);

            // Act
            async Task deserializeAsync() => _ = await linkedListSerializer.Deserialize(stream);

            // Assert
            Assert.DoesNotThrowAsync(deserializeAsync);
        }

        [Test, Order(2)]
        public void Deserialize_StreamWithLinkedListAndUtf8JsonReaderDeserializer_DoesNotThrowException()
        {
            // Arrange
            var linkedListSerializer = new ListSerializer()
            {
                NodeDtosDeserializer = new Utf8JsonReaderNodeDtosDeserializer()
            };
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

        [Test, Order(2)]
        public async Task Deserialize_StreamWithLinkedList_Utf8JsonReaderDeserializerFasterThanCustomDeserializer()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            TimeSpan customDeserializerDeserializationTime, utf8JsonReaderDeserializationTime;
            var nodeDtosDeserializer = new NodeDtosDeserializer();
            var utf8JsonReaderNodeDtosDeserializer = new Utf8JsonReaderNodeDtosDeserializer();
            var linkedListSerializer = new ListSerializer();

            // Act
            linkedListSerializer.NodeDtosDeserializer = nodeDtosDeserializer;
            stopwatch.Start();
            using(var stream = File.OpenRead(_filePath))
                 _ = await linkedListSerializer.Deserialize(stream);
            customDeserializerDeserializationTime = stopwatch.Elapsed;

            linkedListSerializer.NodeDtosDeserializer = utf8JsonReaderNodeDtosDeserializer;
            stopwatch.Restart();
            using (var stream = File.OpenRead(_filePath))
                 _ = await linkedListSerializer.Deserialize(stream);
            utf8JsonReaderDeserializationTime = stopwatch.Elapsed;

            var message = $"""
                            Custom {nameof(NodeDtosDeserializer)} deserialization time: {customDeserializerDeserializationTime.TotalMilliseconds:0} ms
                            {nameof(Utf8JsonReaderNodeDtosDeserializer)} deserialization time: {utf8JsonReaderDeserializationTime.TotalMilliseconds:0} ms
                            """;
            Console.WriteLine(message);

            // Assert
            Assert.That(utf8JsonReaderDeserializationTime, Is.LessThan(customDeserializerDeserializationTime));
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

        private static ListNode GenerateLinkedList(int nodesCount)
        {
            if (nodesCount == 0)
                throw new ArgumentException("Nodes count should be greater than zero!", nameof(nodesCount));

            var nodes = new ListNode[nodesCount];
            var randGen = new Random();

            int dataLength;
            for (var i = 0; i < nodesCount; i++)
            {
                dataLength = randGen.Next(0, 100);

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