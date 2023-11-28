namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class EventModel : IEventModel
{
    public string? Name { get; set; }
    public string? Type { get; set; }

    public List<string> Expressions { get; } = [];
    public string? Expression => this.Expressions.Count > 0 ? this.Expressions[0] : null;
}
