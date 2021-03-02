using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal interface ILocalPropertyExpressionModel : ILocalExpressionModel
    {
        List<string> GetExpressions { get; }
        string? GetExpression { get; }

        List<string> SetExpressions { get; }
        string? SetExpression { get; }
    }
}
