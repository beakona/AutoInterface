namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class StandaloneModel : IRootModel
{
    public string? Interface { get; set; }

    public List<string> References { get; } = new List<string>();
    public string? Reference => this.References.Count > 0 ? this.References[0] : null;

    public List<IMethodModel> Methods { get; } = new List<IMethodModel>();
    public List<IPropertyModel> Properties { get; } = new List<IPropertyModel>();
    public List<IIndexerModel> Indexers { get; } = new List<IIndexerModel>();
    public List<IEventModel> Events { get; } = new List<IEventModel>();
}
