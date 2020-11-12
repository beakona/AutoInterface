using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator
{
    internal interface ISourceTextBuilder
    {
        IBuildContext Context { get; }

        void AppendLine();

        void AppendLine(string? text);

        void Append(string text);

        void Append(object? value);

        void Append(char c);

        void IncrementIndentation();

        void DecrementIndentation();

        void AppendIndentation();

        void AppendIdentifier(ISymbol symbol);

        void AppendTypeReference(ITypeSymbol type, ScopeInfo scope);

        void AppendTypeArgumentsCall(IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope);

        void AppendTypeArgumentsDefinition(IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope);

        void AppendCallParameters(IEnumerable<IParameterSymbol> parameters);

        void AppendMethodDefinition(Compilation compilation, IMethodSymbol method, ScopeInfo scope, INamedTypeSymbol @interface, List<AutoInterfaceInfo> items);

        void AppendPropertyDefinition(IPropertySymbol property, ScopeInfo scope, INamedTypeSymbol @interface, List<AutoInterfaceInfo> items);

        void AppendEventDefinition(IEventSymbol @event, ScopeInfo scope, INamedTypeSymbol @interface, List<AutoInterfaceInfo> items);

        void AppendTypeDeclarationBegin(INamedTypeSymbol type, ScopeInfo scope);

        void AppendNamespaceBegin(string @namespace);
    }
}