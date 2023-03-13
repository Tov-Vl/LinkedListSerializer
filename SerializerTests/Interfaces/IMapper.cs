namespace SerializerTests.Interfaces
{
    public interface IMapper<TSource, TTarget>
    {
        TTarget Map(TSource source);
    }
}
