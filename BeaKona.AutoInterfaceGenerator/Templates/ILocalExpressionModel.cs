using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal interface ILocalExpressionModel
    {
        List<string> Expressions { get; }
        string? Expression { get; }
    }
}
