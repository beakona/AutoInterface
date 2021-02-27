using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class ModelEvent : ModelComponent
    {
        public ModelEvent(Model model, ICodeTextWriter writer, SourceBuilder builder, IEventSymbol @event, ScopeInfo scope) : base(model)
        {
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, @event);
                this.Name = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, @event.Type, scope);
                this.Type = builder2.ToString();
            }
        }

        public string Name { get; }
        public string Type { get; }
    }
}
