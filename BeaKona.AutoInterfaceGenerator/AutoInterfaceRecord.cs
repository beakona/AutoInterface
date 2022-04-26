using BeaKona.AutoInterfaceGenerator.Templates;

namespace BeaKona.AutoInterfaceGenerator;

internal sealed class AutoInterfaceRecord : IMemberInfo
{
    public AutoInterfaceRecord(ISymbol member, ITypeSymbol receiverType, INamedTypeSymbol interfaceType, TemplateDefinition? template, List<PartialTemplate> templateParts, bool bySignature, bool preferCoalesce)
    {
        this.Member = member;
        this.ReceiverType = receiverType;
        this.InterfaceType = interfaceType;
        this.Template = template;
        this.TemplateParts = templateParts?.ToArray() ?? new PartialTemplate[0];
        this.BySignature = bySignature;
        this.PreferCoalesce = preferCoalesce;
    }

    public ISymbol Member { get; }
    public ITypeSymbol ReceiverType { get; }
    public INamedTypeSymbol InterfaceType { get; }
    public TemplateDefinition? Template { get; }
    public PartialTemplate[] TemplateParts { get; }
    public bool BySignature { get; }
    public bool PreferCoalesce { get; }
}
