namespace BeaKona.AutoInterfaceGenerator;

internal static class IDictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) where TKey : notnull where TValue : new()
    {
        if (@this.TryGetValue(key, out var value) == false)
        {
            value = new();
            @this.Add(key, value);
        }

        return value;
    }
}