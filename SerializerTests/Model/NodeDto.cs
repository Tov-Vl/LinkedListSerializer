using System.Text;
using System.Text.Json.Serialization;

namespace SerializerTests.Model
{
    public class NodeDto
    {
        private int _nodeSize;
        private string? _data;

        public const string IndexName = "Index";
        public const string RandomIndexName = "RandomIndex";
        public const string DataName = "Data";
        public const int PropertiesCount = 3;

        [JsonIgnore]
        public int NodeSize { get => _nodeSize; }

        public int Index { get; set; }
        public int? RandomIndex { get; set; }
        public string? Data { get => _data; set { OnDataPropertyChanged(value); _data = value; } }

        private void OnDataPropertyChanged(string? newData)
        {
            _nodeSize = SizeOfIndex()
                + SizeOfRandomIndex()
                + (newData == null ? Encoding.UTF8.GetByteCount(string.Empty) : Encoding.UTF8.GetByteCount(newData!));
        }

        public static int SizeOfIndex() => sizeof(int);

        public static int SizeOfRandomIndex() => sizeof(bool) + sizeof(int);
    }
}
