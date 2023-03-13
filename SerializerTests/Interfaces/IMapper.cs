namespace SerializerTests.Interfaces
{
    internal interface IMapper<TSource, TTarget>
    {
        TTarget Map(TSource source);
    }
}
