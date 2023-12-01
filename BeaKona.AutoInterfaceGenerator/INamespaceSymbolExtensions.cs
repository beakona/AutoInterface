namespace BeaKona.AutoInterfaceGenerator;

internal static class INamespaceSymbolExtensions
{
    public static INamespaceSymbol? FirstNonGlobalNamespace(this INamespaceSymbol @this)
    {
        INamespaceSymbol? last = null;

        for (var n = @this; n.IsGlobalNamespace == false; n = n.ContainingNamespace)
        {
            last = n;
        }

        return last;
    }
}