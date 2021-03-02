using BeaKona.AutoInterfaceGenerator.Templates;
using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal interface IMemberInfo
    {
        ISymbol Member { get; }
        ITypeSymbol ReceiverType { get; }
        INamedTypeSymbol InterfaceType { get; }
        TemplateDefinition? Template { get; }
        PartialTemplate[] TemplateParts { get; }
        bool CastRequired { get; }
    }
}
