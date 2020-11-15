using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal interface IMemberInfo
    {
        ISymbol Member { get; }
        INamedTypeSymbol InterfaceType { get; }
        bool CastRequired { get; }
        bool AllowNullConditionOperator { get; }
    }
}
