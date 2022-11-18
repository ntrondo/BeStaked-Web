namespace UtilitiesLib.ConvertPrimitives.BaseClasses
{
    public abstract class CachedConverterBase<S, T> : Interfaces.IConvert<S, T> where S:notnull
    {
        private readonly IDictionary<S, T> cache = new Dictionary<S,T>();

        public T? Convert(S value)
        {
            if (IsDefault(value))
                return GetDefaultValue();
            if (cache.ContainsKey(value))
                return cache[value];
            T newValue = convert(value);
            cache[value] = newValue;
            return newValue;
        }

        protected virtual T? GetDefaultValue()
        {
            return default;
        }

        protected virtual bool IsDefault(S value)
        {
            return EqualityComparer<S>.Default.Equals(value, default);
        }

        public abstract T convert(S value);
    }
}
