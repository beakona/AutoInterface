using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class ModelIndexer : ModelProperty
    {
        public ModelIndexer(Model model, ICodeTextWriter writer, SourceBuilder builder, IPropertySymbol indexer, ScopeInfo scope) : base(model, writer, builder, indexer, scope)
        {
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteParameterDefinition(builder2, scope, indexer.Parameters);
                this.ParametersDefinition = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteCallParameters(builder2, indexer.Parameters);
                this.CallParameters = builder2.ToString();
            }
        }

        public string ParametersDefinition { get; }
        public string CallParameters { get; }
    }
}
