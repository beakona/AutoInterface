using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal static class ModelLoader
    {
        public static void Load(this IMethodModel @this, ICodeTextWriter writer, SourceBuilder builder, IMethodSymbol method, ScopeInfo scope, IEnumerable<IMemberInfo> references)
        {
            ScopeInfo methodScope = new ScopeInfo(scope);

            if (method.IsGenericMethod)
            {
                methodScope.CreateAliases(method.TypeArguments);
            }

            (bool isAsync, bool methodReturnsValue) = SemanticFacts.IsAsyncAndGetReturnType(writer.Compilation, method);
            bool canUseAsync = true;

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, method);
                if (method.IsGenericMethod)
                {
                    builder2.Append('<');
                    writer.WriteTypeArgumentsCall(builder2, method.TypeArguments, methodScope);
                    builder2.Append('>');
                }
                @this.Name = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, method.ReturnType, methodScope);
                @this.ReturnType = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteParameterDefinition(builder2, methodScope, method.Parameters);
                @this.ArgumentsDefinition = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteCallParameters(builder2, method.Parameters);
                @this.CallArguments = builder2.ToString();
            }

            @this.IsAsync = isAsync && canUseAsync;
            @this.ReturnExpected = (isAsync && methodReturnsValue == false) ? canUseAsync == false : methodReturnsValue;

            foreach (IMemberInfo reference in references)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                if (isAsync && canUseAsync)
                {
                    writer.WriteMethodCall(builder2, reference, method, methodScope, false, false, false);
                }
                else
                {
                    writer.WriteMethodCall(builder2, reference, method, methodScope, false, SemanticFacts.IsNullable(writer.Compilation, method.ReturnType), true);
                }
                @this.Expressions.Add(builder2.ToString());
            }
        }

        public static void Load(this IPropertyModel @this, ICodeTextWriter writer, SourceBuilder builder, IPropertySymbol property, ScopeInfo scope, IEnumerable<IMemberInfo> references)
        {
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, property);
                @this.Name = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, property.Type, scope);
                @this.Type = builder2.ToString();
            }

            @this.HaveGetter = property.GetMethod != null;
            @this.HaveSetter = property.SetMethod != null;

            foreach (IMemberInfo reference in references)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WritePropertyCall(builder2, reference, property, scope, SemanticFacts.IsNullable(writer.Compilation, property.Type), false);
                @this.Expressions.Add(builder2.ToString());
            }

            if (@this is ILocalPropertyExpressionModel this2)
            {
                this2.Load(writer, builder, property, scope, references);
            }
        }

        public static void Load(this ILocalPropertyExpressionModel @this, ICodeTextWriter writer, SourceBuilder builder, IPropertySymbol property, ScopeInfo scope, IEnumerable<IMemberInfo> references)
        {
            foreach (IMemberInfo reference in references)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WritePropertyCall(builder2, reference, property, scope, SemanticFacts.IsNullable(writer.Compilation, property.Type), true);
                @this.GetExpressions.Add(builder2.ToString());
            }

            foreach (IMemberInfo reference in references)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WritePropertyCall(builder2, reference, property, scope, false, false);
                @this.SetExpressions.Add(builder2.ToString());
            }
        }

        public static void Load(this IIndexerModel @this, ICodeTextWriter writer, SourceBuilder builder, IPropertySymbol indexer, ScopeInfo scope, IEnumerable<IMemberInfo> references)
        {
            (@this as IPropertyModel).Load(writer, builder, indexer, scope, references);

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteParameterDefinition(builder2, scope, indexer.Parameters);
                @this.ParametersDefinition = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteCallParameters(builder2, indexer.Parameters);
                @this.CallParameters = builder2.ToString();
            }
        }

        public static void Load(this IEventModel @this, ICodeTextWriter writer, SourceBuilder builder, IEventSymbol @event, ScopeInfo scope, IEnumerable<IMemberInfo> references)
        {
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, @event);
                @this.Name = builder2.ToString();
            }

            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, @event.Type, scope);
                @this.Type = builder2.ToString();
            }

            foreach (IMemberInfo reference in references)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteMemberReference(builder2, reference, scope, false, false);
                builder2.Append('.');
                writer.WriteIdentifier(builder2, @event);
                @this.Expressions.Add(builder2.ToString());
            }
        }

        public static void Load(this IRootModel @this, ICodeTextWriter writer, SourceBuilder builder, INamedTypeSymbol @interface, ScopeInfo scope, IEnumerable<IMemberInfo> references)
        {
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteTypeReference(builder2, @interface, scope);
                @this.Interface = builder2.ToString();
            }

            @this.References.AddRange(references.Select(delegate (IMemberInfo member)
            {
                SourceBuilder builder2 = builder.AppendNewBuilder(false);
                writer.WriteIdentifier(builder2, member.Member);
                return builder2.ToString();
            }));
        }
    }
}
