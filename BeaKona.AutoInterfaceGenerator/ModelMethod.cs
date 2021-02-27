using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class ModelMethod : ModelComponent
    {
        public ModelMethod(Model model, ICodeTextWriter writer, SourceBuilder builder, IMethodSymbol method, ScopeInfo scope) : base(model)
        {
            ScopeInfo methodScope = new ScopeInfo(scope);

            if (method.IsGenericMethod)
            {
                methodScope.CreateAliases(method.TypeArguments);
            }

            (bool isAsync, bool methodReturnsValue) = SemanticFacts.IsAsyncAndGetReturnType(writer.Compilation, method);
            bool useAsync = true;

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, method);
                if (method.IsGenericMethod)
                {
                    builder2.Append('<');
                    writer.WriteTypeArgumentsCall(builder2, method.TypeArguments, methodScope);
                    builder2.Append('>');
                }
                this.Name = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, method.ReturnType, methodScope);
                this.ReturnType = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteParameterDefinition(builder2, methodScope, method.Parameters);
                this.ArgumentsDefinition = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteCallParameters(builder2, method.Parameters);
                this.CallArguments = builder2.ToString();
            }

            this.IsAsync = isAsync && useAsync;
            this.ReturnExpected = (isAsync && methodReturnsValue == false) ? useAsync == false : methodReturnsValue;
        }

        public string Name { get; }
        public string ArgumentsDefinition { get; }
        public string CallArguments { get; }
        public string ReturnType { get; }
        public bool IsAsync { get; }
        public bool ReturnExpected { get; }
    }
}
