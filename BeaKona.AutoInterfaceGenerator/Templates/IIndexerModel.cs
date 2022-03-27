namespace BeaKona.AutoInterfaceGenerator.Templates;

internal interface IIndexerModel : IPropertyModel
{
    string? ParametersDefinition { get; set; }
    string? CallParameters { get; set; }
}
