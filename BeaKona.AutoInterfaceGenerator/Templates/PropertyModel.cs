using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal class PropertyModel : IPropertyModel
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public bool HaveGetter { get; set; }
        public bool HaveSetter { get; set; }

        public List<string> Expressions { get; } = new List<string>();
        public string? Expression => this.Expressions.Count > 0 ? this.Expressions[0] : null;

        public List<string> GetExpressions { get; } = new List<string>();
        public string? GetExpression => this.GetExpressions.Count > 0 ? this.GetExpressions[0] : null;

        public List<string> SetExpressions { get; } = new List<string>();
        public string? SetExpression => this.SetExpressions.Count > 0 ? this.SetExpressions[0] : null;
    }
}
