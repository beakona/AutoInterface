namespace BeaKona.AutoInterfaceGenerator;

public static class INamedTypeSymbolExtensions
{
    public static AttributeTargets? GetAttributeUsages(this INamedTypeSymbol @this)
    {
        foreach (var attribute in @this.GetAttributes())
        {
            if (attribute.AttributeClass is INamedTypeSymbol attributeClass && attributeClass.ToDisplayString() == "System.AttributeUsageAttribute")
            {
                return (AttributeTargets)Convert.ToInt32(attribute.ConstructorArguments[0].Value);
            }
        }

        return default;
    }
}
