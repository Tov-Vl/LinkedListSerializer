namespace SerializerTests.Model
{
    public class NodeDto
    {
        public const string IndexName = "Index";
        public const string RandomIndexName = "RandomIndex";
        public const string DataName = "Data";
        public const int PropertiesCount = 3;

        public int Index { get; set; }
        public int? RandomIndex { get; set; }
        public string? Data { get; set; }
    }
}
