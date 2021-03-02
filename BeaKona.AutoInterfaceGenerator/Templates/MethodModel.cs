using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal class MethodModel : IMethodModel
    {
        public string? Name { get; set; }
        public string? ArgumentsDefinition { get; set; }
        public string? CallArguments { get; set;  }
        public string? ReturnType { get; set; }
        public bool IsAsync { get; set; }
        public bool ReturnExpected { get; set; }

        public List<string> Expressions { get; } = new List<string>();
        public string? Expression => this.Expressions.Count > 0 ? this.Expressions[0] : null;
    }
}
