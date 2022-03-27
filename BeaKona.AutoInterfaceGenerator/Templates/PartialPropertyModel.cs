namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class PartialPropertyModel : PropertyModel, IRootModel
{
    public string? Interface { get; set; }

    public List<string> References { get; } = new List<string>();
    public string? Reference => this.References.Count > 0 ? this.References[0] : null;
}
