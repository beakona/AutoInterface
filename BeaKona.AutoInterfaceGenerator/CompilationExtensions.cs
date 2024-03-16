namespace BeaKona.AutoInterfaceGenerator;

internal static class CompilationExtensions
{
    public static INamespaceSymbol? GetNamespaceByMetadataName(this Compilation @this, string name)
    {
        var parts = name.Split(new char[] { '.' }, StringSplitOptions.None);

        var @namespace = @this.GlobalNamespace;
        foreach (var part in parts)
        {
            var next = @namespace.GetNamespace(part);
            if (next == null)
            {
                return null;
            }
            @namespace = next;
        }

        return @namespace;
    }

    public static bool IsVisible(this Compilation @this, INamedTypeSymbol className)
    {
        foreach (var type in @this.GetTypesByMetadataName(className.ToDisplayString()))
        {
            if (type.DeclaredAccessibility == Accessibility.Public)
            {
                return true;
            }
            else if (type.DeclaredAccessibility == Accessibility.Internal)
            {
                if (@this.Assembly.Equals(type.ContainingAssembly, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }
        }

        return false;
    }
}