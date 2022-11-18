namespace UtilitiesLib.Models.Interfaces
{
    public interface IExposeScale
    {
        INamedScale Scale { get; }
    }
    public interface IScaledAmount:IExposeAmount<double>, IExposeScale { }
}
