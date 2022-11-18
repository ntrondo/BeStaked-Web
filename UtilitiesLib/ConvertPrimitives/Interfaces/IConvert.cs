namespace UtilitiesLib.ConvertPrimitives.Interfaces
{
    public interface IConvert<S,T>
    {
        T Convert(S value);
    }
}
