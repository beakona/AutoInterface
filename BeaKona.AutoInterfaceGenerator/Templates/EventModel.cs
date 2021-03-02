using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal class EventModel : IEventModel
    {
        public string? Name { get; set; }
        public string? Type { get; set; }

        public List<string> Expressions { get; } = new List<string>();
        public string? Expression => this.Expressions.Count > 0 ? this.Expressions[0] : null;
    }
}
