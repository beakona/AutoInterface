namespace BeaKona.AutoInterfaceGenerator;

internal interface ICodeTextWriter
{
    Compilation Compilation { get; }

    void WriteTypeReference(SourceBuilder builder, ITypeSymbol type, ScopeInfo scope);

    void WriteTypeArgumentsCall(SourceBuilder builder, IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope);

    void WriteTypeArgumentsDefinition(SourceBuilder builder, IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope);

    void WriteParameterAttributes(SourceBuilder builder, ScopeInfo scope, IParameterSymbol parameter);

    void WriteParameterDefinition(SourceBuilder builder, ScopeInfo scope, IEnumerable<IParameterSymbol> parameters);

    void WriteCallParameters(SourceBuilder builder, IEnumerable<IParameterSymbol> parameters);

    void WriteMethodDefinition(SourceBuilder builder, IMethodSymbol method, ScopeInfo scope, INamedTypeSymbol @interface, IEnumerable<IMemberInfo> references);

    void WritePropertyDefinition(SourceBuilder builder, IPropertySymbol property, ScopeInfo scope, INamedTypeSymbol @interface, IEnumerable<IMemberInfo> references);

    void WriteEventDefinition(SourceBuilder builder, IEventSymbol @event, ScopeInfo scope, INamedTypeSymbol @interface, IEnumerable<IMemberInfo> references);

    void WriteTypeDeclarationBeginning(SourceBuilder builder, INamedTypeSymbol type, ScopeInfo scope);

    bool WriteNamespaceBeginning(SourceBuilder builder, INamespaceSymbol @namespace);

    void WriteHolderReference(SourceBuilder builder, ISymbol member, ScopeInfo scope);

    void WriteMemberReference(SourceBuilder builder, IMemberInfo reference, ScopeInfo scope, bool typeIsNullable, bool allowCoalescing);

    void WritePropertyCall(SourceBuilder builder, IMemberInfo reference, IPropertySymbol property, ScopeInfo scope, bool typeIsNullable, bool allowCoalescing);

    void WriteMethodCall(SourceBuilder builder, IMemberInfo reference, IMethodSymbol method, ScopeInfo scope, bool async, bool typeIsNullable, bool allowCoalescing);

    void WriteIdentifier(SourceBuilder builder, ISymbol symbol);

    void WriteRefKind(SourceBuilder builder, RefKind kind, bool dynamicExists);
}
