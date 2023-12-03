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

    public static INamespaceSymbol[] GetNamespaceElements(this INamespaceSymbol @this)
    {
        List<INamespaceSymbol> containingNamespaces = [];
        for (INamespaceSymbol? n = @this; n != null && n.IsGlobalNamespace == false; n = n.ContainingNamespace)
        {
            containingNamespaces.Insert(0, n);
        }
        return [.. containingNamespaces];
    }
}
