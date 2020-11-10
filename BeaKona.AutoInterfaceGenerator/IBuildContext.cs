using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator
{
    public interface IBuildContext
    {
        bool IsVerbatim(ISymbol symbol);
        bool IsNullableT(INamedTypeSymbol type);

        string GetSourceIdentifier(ISymbol symbol);

        ImmutableArray<INamespaceSymbol> GetNamespaceParts(INamespaceSymbol @namespace);
    }
}
