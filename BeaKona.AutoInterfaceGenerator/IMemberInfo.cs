using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal interface IMemberInfo
    {
        bool CastRequired { get; }
        ISymbol Member { get; }
        INamedTypeSymbol InterfaceType { get; }
    }
}
