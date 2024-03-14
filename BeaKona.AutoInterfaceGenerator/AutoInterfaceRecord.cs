using BeaKona.AutoInterfaceGenerator.Templates;

namespace BeaKona.AutoInterfaceGenerator;

internal sealed class AutoInterfaceRecord(ISymbol member, ITypeSymbol receiverType, INamedTypeSymbol interfaceType, TemplateDefinition? template, IEnumerable<PartialTemplate> templateParts, bool bySignature, bool preferCoalesce, bool allowMissingMembers, MemberMatchTypes memberMatch) : IMemberInfo
{
    public ISymbol Member { get; } = member;
    public ITypeSymbol ReceiverType { get; } = receiverType;
    public INamedTypeSymbol InterfaceType { get; } = interfaceType;
    public TemplateDefinition? Template { get; } = template;
    public PartialTemplate[] TemplateParts { get; } = templateParts?.ToArray() ?? [];
    public bool BySignature { get; } = bySignature;
    public bool PreferCoalesce { get; } = preferCoalesce;
    public bool AllowMissingMembers { get; } = allowMissingMembers;
    public MemberMatchTypes MemberMatch { get; } = memberMatch;
}
