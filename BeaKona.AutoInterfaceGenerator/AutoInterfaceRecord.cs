using BeaKona.AutoInterfaceGenerator.Templates;

namespace BeaKona.AutoInterfaceGenerator;

internal sealed class AutoInterfaceRecord : IMemberInfo
{
    public AutoInterfaceRecord(ISymbol member, ITypeSymbol receiverType, INamedTypeSymbol interfaceType, TemplateDefinition? template, List<PartialTemplate> templateParts, bool bySignature, bool preferCoalesce, bool allowMissingMembers)
    {
        this.Member = member;
        this.ReceiverType = receiverType;
        this.InterfaceType = interfaceType;
        this.Template = template;
        this.TemplateParts = templateParts?.ToArray() ?? [];
        this.BySignature = bySignature;
        this.PreferCoalesce = preferCoalesce;
        this.AllowMissingMembers = allowMissingMembers;
    }

    public ISymbol Member { get; }
    public ITypeSymbol ReceiverType { get; }
    public INamedTypeSymbol InterfaceType { get; }
    public TemplateDefinition? Template { get; }
    public PartialTemplate[] TemplateParts { get; }
    public bool BySignature { get; }
    public bool PreferCoalesce { get; }
    public bool AllowMissingMembers { get; }
}
