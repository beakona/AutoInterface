using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class Model
    {
        public Model(ICodeTextWriter writer, SourceBuilder builder, INamedTypeSymbol @interface, INamedTypeSymbol type, IEnumerable<IMemberInfo> references, ScopeInfo scope)
        {
            SourceBuilder builder2 = builder.AppendNewBuilder(false);
            writer.WriteTypeReference(builder2, @interface, scope);
            this.Interface = builder2.ToString();

            this.References.AddRange(references.Select(delegate (IMemberInfo member)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, member.Member);
                return builder2.ToString();
            }));

            foreach (IMethodSymbol method in @interface.GetMethods().Where(i => type.IsMemberImplemented(i) == false))
            {
                this.Methods.Add(new ModelMethod(this, writer, builder, method, scope));
            }

            foreach (IPropertySymbol property in @interface.GetProperties().Where(i => type.IsMemberImplemented(i) == false))
            {
                this.Properties.Add(new ModelProperty(this, writer, builder, property, scope));
            }

            foreach (IPropertySymbol indexer in @interface.GetIndexers().Where(i => type.IsMemberImplemented(i) == false))
            {
                this.Indexers.Add(new ModelIndexer(this, writer, builder, indexer, scope));
            }

            foreach (IEventSymbol @event in @interface.GetEvents().Where(i => type.IsMemberImplemented(i) == false))
            {
                this.Events.Add(new ModelEvent(this, writer, builder, @event, scope));
            }
        }

        public string Interface { get; }

        public List<string> References { get; } = new List<string>();

        public List<ModelMethod> Methods = new List<ModelMethod>();
        public List<ModelProperty> Properties = new List<ModelProperty>();
        public List<ModelIndexer> Indexers = new List<ModelIndexer>();
        public List<ModelEvent> Events = new List<ModelEvent>();
    }
}
