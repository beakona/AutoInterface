namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class StandaloneModel : IRootModel
{
    public string? Interface { get; set; }

    public List<string> References { get; } = [];
    public string? Reference => this.References.Count > 0 ? this.References[0] : null;

    public List<IMethodModel> Methods { get; } = [];
    public List<IPropertyModel> Properties { get; } = [];
    public List<IIndexerModel> Indexers { get; } = [];
    public List<IEventModel> Events { get; } = [];
}
