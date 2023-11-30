using BeaKona.AutoInterfaceGenerator.Templates;

namespace BeaKona.AutoInterfaceGenerator;

internal interface IMemberInfo
{
    ISymbol Member { get; }
    ITypeSymbol ReceiverType { get; }
    INamedTypeSymbol InterfaceType { get; }
    TemplateDefinition? Template { get; }
    PartialTemplate[] TemplateParts { get; }
    bool BySignature { get; }
    bool PreferCoalesce { get; }
    bool AllowMissingMembers { get; }
    MemberMatchTypes MemberMatch { get; }
}
