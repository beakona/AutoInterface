namespace BeaKona.AutoInterfaceGenerator.Templates;

internal interface IEventModel : ILocalExpressionModel
{
    string? Name { get; set; }
    string? Type { get; set; }
}
