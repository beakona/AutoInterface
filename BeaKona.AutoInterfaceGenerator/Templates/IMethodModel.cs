namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal interface IMethodModel : ILocalExpressionModel
    {
        string? Name { get; set; }
        string? ArgumentsDefinition { get; set; }
        string? CallArguments { get; set; }
        string? ReturnType { get; set; }
        bool IsAsync { get; set; }
        bool ReturnExpected { get; set; }
    }
}
