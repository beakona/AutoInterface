using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class CSharpBuildContext : IBuildContext
    {
        public CSharpBuildContext(Compilation compilation)
        {
            this.symbolNullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T);
        }

        private readonly INamedTypeSymbol symbolNullableT;

        public bool IsVerbatim(ISymbol symbol)
        {
            foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                SyntaxNode syntax = syntaxReference.GetSyntax();
                if (syntax is BaseTypeDeclarationSyntax type)
                {
                    return type.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is MethodDeclarationSyntax method)
                {
                    return method.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is ParameterSyntax parameter)
                {
                    return parameter.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is VariableDeclaratorSyntax variableDeclarator)
                {
                    return variableDeclarator.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is VariableDeclarationSyntax variableDeclaration)
                {
                    foreach (var variable in variableDeclaration.Variables)
                    {
                        if (variable.Identifier.IsVerbatimIdentifier())
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else if (syntax is BaseFieldDeclarationSyntax field)
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        if (variable.Identifier.IsVerbatimIdentifier())
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else if (syntax is PropertyDeclarationSyntax property)
                {
                    return property.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is EventDeclarationSyntax @event)
                {
                    return @event.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is TypeParameterSyntax typeParameter)
                {
                    return typeParameter.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is TupleTypeSyntax)
                {
                    return false;
                }
                else if (syntax is TupleElementSyntax tupleElement)
                {
                    return tupleElement.Identifier.IsVerbatimIdentifier();
                }
                else if (syntax is NamespaceDeclarationSyntax @namespace)
                {
                    if (@namespace.Name is IdentifierNameSyntax identifier)
                    {
                        return identifier.Identifier.IsVerbatimIdentifier();
                    }
                    else
                    {
                        throw new NotSupportedException(syntax.GetType().ToString());
                    }
                }
                else
                {
                    throw new NotSupportedException(syntax.GetType().ToString());
                }
            }

            return false;
        }

        public bool IsNullableT(INamedTypeSymbol type)
        {
            if (type.IsGenericType && type.IsUnboundGenericType == false)
            {
                return this.symbolNullableT.ConstructUnboundGenericType().Equals(type.ConstructUnboundGenericType(), SymbolEqualityComparer.Default);
            }

            return false;
        }

        public string GetSourceIdentifier(ISymbol symbol)
        {
            if (this.IsVerbatim(symbol))
            {
                return $"@{symbol.Name}";
            }
            else
            {
                return symbol.Name;
            }
        }

        public ImmutableArray<INamespaceSymbol> GetNamespaceParts(INamespaceSymbol @namespace)
        {
            List<INamespaceSymbol> parts = new List<INamespaceSymbol>();
            for (; @namespace != null && @namespace.IsGlobalNamespace == false; @namespace = @namespace.ContainingNamespace)
            {
                parts.Insert(0, @namespace);
            }
            return parts.ToImmutableArray();
        }
    }
}
