using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal interface IMemberInfo
    {
        ISymbol Member { get; }
        ITypeSymbol ReceiverType { get; }
        AttributeData Attribute { get; }
        INamedTypeSymbol InterfaceType { get; }
        TemplateSettings? Template { get; }
        bool CastRequired { get; }
    }
}
