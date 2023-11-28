namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class PartialPropertyModel : PropertyModel, IRootModel
{
    public string? Interface { get; set; }

    public List<string> References { get; } = [];
    public string? Reference => this.References.Count > 0 ? this.References[0] : null;
}
