using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator
{
    internal interface ICodeTextBuilder
    {
        Compilation Compilation { get; }
        SourceBuilder Builder { get; }

        void AppendTypeReference(ITypeSymbol type, ScopeInfo scope);

        void AppendTypeArgumentsCall(IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope);

        void AppendTypeArgumentsDefinition(IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope);

        void AppendCallParameters(IEnumerable<IParameterSymbol> parameters);

        void AppendMethodDefinition(IMethodSymbol method, ScopeInfo scope, INamedTypeSymbol @interface, List<IMemberInfo> items);

        void AppendPropertyDefinition(IPropertySymbol property, ScopeInfo scope, INamedTypeSymbol @interface, List<IMemberInfo> items);

        void AppendEventDefinition(IEventSymbol @event, ScopeInfo scope, INamedTypeSymbol @interface, List<IMemberInfo> items);

        void AppendTypeDeclarationBeginning(INamedTypeSymbol type, ScopeInfo scope);

        void AppendNamespaceBeginning(string @namespace);

        void AppendHolderReference(ISymbol member, ScopeInfo scope);

        void AppendMemberReference(IMemberInfo item, ScopeInfo scope);

        void AppendMethodCall(IMemberInfo item, IMethodSymbol method, ScopeInfo scope, bool async);

        string GetSourceIdentifier(ISymbol symbol);
    }
}