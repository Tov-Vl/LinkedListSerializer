namespace SerializerTests.Options
{
    public class ListSerializerOptions
    {
        // Number of nodes after which serialization using System.Text.Json.JsonSerializer will be faster
        public const int CustomSerializationThreshold = 400_000;
    }
}
