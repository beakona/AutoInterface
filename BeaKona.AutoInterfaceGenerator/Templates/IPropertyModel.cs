namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal interface IPropertyModel : ILocalPropertyExpressionModel
    {
        string? Name { get; set; }
        string? Type { get; set; }
        bool HaveGetter { get; set; }
        bool HaveSetter { get; set; }
    }
}
