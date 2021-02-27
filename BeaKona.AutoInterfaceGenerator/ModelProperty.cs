using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class ModelProperty : ModelComponent
    {
        public ModelProperty(Model model, ICodeTextWriter writer, SourceBuilder builder, IPropertySymbol property, ScopeInfo scope) : base(model)
        {
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, property);
                this.Name = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, property.Type, scope);
                this.Type = builder2.ToString();
            }

            this.HaveGetter = property.GetMethod != null;
            this.HaveSetter = property.SetMethod != null;
        }

        public string Name { get; }
        public string Type { get; }
        public bool HaveGetter { get; }
        public bool HaveSetter { get; }
    }
}
