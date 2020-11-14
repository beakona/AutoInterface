using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class CSharpCodeTextBuilder : ICodeTextBuilder
    {
        public CSharpCodeTextBuilder(Compilation compilation, SourceBuilder builder)
        {
            this.Compilation = compilation;
            this.Builder = builder;
            this.aliasSection = builder.AppendNewBuilder();
        }

        public Compilation Compilation { get; }
        public SourceBuilder Builder { get; }

        private readonly HashSet<string> aliases = new HashSet<string>();
        private readonly SourceBuilder aliasSection;

        public override string ToString()
        {
            if (this.aliases.Count == 0)
            {
                return Builder.ToString();
            }

            this.aliasSection.Clear();
            foreach (string alias in this.aliases.OrderByDescending(i => i))
            {
                this.aliasSection.Append("extern alias ");
                this.aliasSection.Append(alias);
                this.aliasSection.AppendLine(';');
            }

            return this.Builder.ToString();
        }

        public void AppendTypeReference(ITypeSymbol type, ScopeInfo scope)
        {
            if (scope.TryGetAlias(type, out string? typeName))
            {
                if (typeName != null)
                {
                    this.Builder.Append(typeName);
                }
            }
            else
            {
                bool processed = false;
                if (type.SpecialType != SpecialType.None)
                {
                    processed = true;
                    switch (type.SpecialType)
                    {
                        default: processed = false; break;
                        case SpecialType.System_Object: this.Builder.Append("object"); break;
                        case SpecialType.System_Void: this.Builder.Append("void"); break;
                        case SpecialType.System_Boolean: this.Builder.Append("bool"); break;
                        case SpecialType.System_Char: this.Builder.Append("char"); break;
                        case SpecialType.System_SByte: this.Builder.Append("sbyte"); break;
                        case SpecialType.System_Byte: this.Builder.Append("byte"); break;
                        case SpecialType.System_Int16: this.Builder.Append("short"); break;
                        case SpecialType.System_UInt16: this.Builder.Append("ushort"); break;
                        case SpecialType.System_Int32: this.Builder.Append("int"); break;
                        case SpecialType.System_UInt32: this.Builder.Append("uint"); break;
                        case SpecialType.System_Int64: this.Builder.Append("long"); break;
                        case SpecialType.System_UInt64: this.Builder.Append("ulong"); break;
                        case SpecialType.System_Decimal: this.Builder.Append("decimal"); break;
                        case SpecialType.System_Single: this.Builder.Append("float"); break;
                        case SpecialType.System_Double: this.Builder.Append("double"); break;
                        //case SpecialType.System_Half: this.output.Append("half"); break;
                        case SpecialType.System_String: this.Builder.Append("string"); break;
                    }
                }

                if (processed == false)
                {
                    if (type is IArrayTypeSymbol array)
                    {
                        this.AppendTypeReference(array.ElementType, scope);
                        this.Builder.Append('[');
                        for (int i = 1; i < array.Rank; i++)
                        {
                            this.Builder.Append(',');
                        }
                        this.Builder.Append(']');
                    }
                    else
                    {
                        static bool IsTupleWithAliases(INamedTypeSymbol tuple)
                        {
                            return tuple.TupleElements.Any(i => i.CorrespondingTupleField != null && i.Equals(i.CorrespondingTupleField, SymbolEqualityComparer.Default) == false);
                        }

                        if (type.IsTupleType && type is INamedTypeSymbol tupleType && IsTupleWithAliases(tupleType))
                        {
                            this.Builder.Append('(');
                            bool first = true;
                            foreach (IFieldSymbol field in tupleType.TupleElements)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    this.Builder.Append(", ");
                                }
                                this.AppendTypeReference(field.Type, scope);
                                this.Builder.Append(' ');
                                this.AppendIdentifier(field);
                            }
                            this.Builder.Append(')');
                        }
                        else if (type is INamedTypeSymbol nt && SemanticFacts.IsNullableT(this.Compilation, nt))
                        {
                            this.AppendTypeReference(nt.TypeArguments[0], scope);
                        }
                        else
                        {
                            if (type is ITypeParameterSymbol == false)
                            {
                                if (type.Equals(scope.Type, SymbolEqualityComparer.Default) == false)
                                {
                                    string? alias = SemanticFacts.ResolveAssemblyAlias(this.Compilation, type.ContainingAssembly);
                                    ISymbol[] symbols;
                                    if (alias == null)
                                    {
                                        symbols = SemanticFacts.GetRelativeSymbols(type, scope.Type);
                                    }
                                    else
                                    {
                                        symbols = SemanticFacts.GetContainingSymbols(type, false);
                                        this.Builder.Append(alias);
                                        this.Builder.Append("::");
                                        this.aliases.Add(alias);
                                    }

                                    foreach (ISymbol symbol in symbols)
                                    {
                                        this.AppendIdentifier(symbol);

                                        if (symbol is INamedTypeSymbol snt && snt.IsGenericType)
                                        {
                                            this.Builder.Append('<');
                                            this.AppendTypeArgumentsDefinition(snt.TypeArguments, scope);
                                            this.Builder.Append('>');
                                        }

                                        this.Builder.Append('.');
                                    }
                                }
                            }

                            this.AppendIdentifier(type);

                            {
                                if (type is INamedTypeSymbol tnt && tnt.IsGenericType)
                                {
                                    this.Builder.Append('<');
                                    this.AppendTypeArgumentsDefinition(tnt.TypeArguments, scope);
                                    this.Builder.Append('>');
                                }
                            }
                        }
                    }
                }
            }

            if (type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                this.Builder.Append('?');
            }
        }

        public void AppendTypeArgumentsCall(IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope)
        {
            bool first = true;
            foreach (ITypeSymbol t in typeArguments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    this.Builder.Append(", ");
                }

                if (scope.TryGetAlias(t, out string? alias))
                {
                    if (alias != null)
                    {
                        this.Builder.Append(alias);
                    }
                }
                else
                {
                    this.AppendIdentifier(t);
                }
            }
        }

        public void AppendTypeArgumentsDefinition(IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope)
        {
            bool first = true;
            foreach (ITypeSymbol t in typeArguments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    this.Builder.Append(", ");
                }
                this.AppendTypeReference(t, scope);
            }
        }

        public void AppendCallParameters(IEnumerable<IParameterSymbol> parameters)
        {
            bool first = true;
            foreach (IParameterSymbol parameter in parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    this.Builder.Append(", ");
                }
                this.AppendRefKind(parameter.RefKind);
                this.Builder.AppendSpaceIfNeccessary();
                this.AppendIdentifier(parameter);
            }
        }

        public void AppendMethodDefinition(IMethodSymbol method, ScopeInfo scope, INamedTypeSymbol @interface, List<IMemberInfo> items)
        {
            ScopeInfo methodScope = new ScopeInfo(scope);

            if (method.IsGenericMethod)
            {
                methodScope.CreateAliases(method.TypeArguments);
            }

            (bool isAsync, bool methodReturnsValue) = SemanticFacts.IsAsyncAndGetReturnType(this.Compilation, method);

            bool useAsync = items.Count > 1;
            bool returnsValue = (isAsync && methodReturnsValue == false) ? useAsync == false : methodReturnsValue;

            this.Builder.AppendIndentation();
            if (isAsync && useAsync)
            {
                this.Builder.Append("async");
                this.Builder.Append(' ');
            }
            this.AppendTypeReference(method.ReturnType, methodScope);
            this.Builder.Append(' ');
            this.AppendTypeReference(@interface, scope);
            this.Builder.Append('.');
            this.AppendIdentifier(method);
            if (method.IsGenericMethod)
            {
                this.Builder.Append('<');
                this.AppendTypeArgumentsCall(method.TypeArguments, methodScope);
                this.Builder.Append('>');
            }
            this.Builder.Append('(');
            bool first = true;
            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    this.Builder.Append(", ");
                }

                if (parameter.IsParams)
                {
                    this.Builder.Append("params");
                    this.Builder.Append(' ');
                }
                else
                {
                    this.AppendRefKind(parameter.RefKind);
                    this.Builder.AppendSpaceIfNeccessary();
                }
                this.AppendTypeReference(parameter.Type, methodScope);
                this.Builder.Append(' ');
                this.AppendIdentifier(parameter);
                if (parameter.HasExplicitDefaultValue)
                {
                    this.Builder.Append(" = ");
                    this.Builder.Append(parameter.ExplicitDefaultValue ?? "null");
                }
            }
            this.Builder.Append(")");

            if (items.Count == 1)
            {
                this.Builder.Append(" => ");
                this.AppendMethodCall(items[0], method, methodScope, isAsync && useAsync);
                this.Builder.AppendLine(';');
            }
            else
            {
                this.Builder.AppendLine();
                this.Builder.AppendIndentation();
                this.Builder.AppendLine('{');
                for (int m = 0; m < items.Count; m++)
                {
                    this.Builder.IncrementIndentation();
                    try
                    {
                        this.Builder.AppendIndentation();
                        if (returnsValue && m + 1 == items.Count)
                        {
                            this.Builder.Append("return");
                            this.Builder.Append(' ');
                        }
                        this.AppendMethodCall(items[m], method, methodScope, isAsync && useAsync);
                        this.Builder.AppendLine(';');
                    }
                    finally
                    {
                        this.Builder.DecrementIndentation();
                    }
                }
                this.Builder.AppendIndentation();
                this.Builder.AppendLine('}');
            }
        }

        public void AppendPropertyDefinition(IPropertySymbol property, ScopeInfo scope, INamedTypeSymbol @interface, List<IMemberInfo> items)
        {
            this.Builder.AppendIndentation();
            this.AppendTypeReference(property.Type, scope);
            this.Builder.Append(' ');
            this.AppendTypeReference(@interface, scope);
            this.Builder.Append('.');

            if (property.IsIndexer)
            {
                this.Builder.Append("this[");
                bool first = true;
                foreach (IParameterSymbol parameter in property.Parameters)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        this.Builder.Append(", ");
                    }
                    this.AppendRefKind(parameter.RefKind);
                    this.Builder.AppendSpaceIfNeccessary();
                    this.AppendTypeReference(parameter.Type, scope);
                    this.Builder.Append(' ');
                    this.AppendIdentifier(parameter);
                }
                this.Builder.Append(']');
            }
            else
            {
                this.AppendIdentifier(property);
            }

            void AppendDefinition(IPropertySymbol property)
            {
                if (property.IsIndexer)
                {
                    this.Builder.Append('[');
                    this.AppendCallParameters(property.Parameters);
                    this.Builder.Append(']');
                }
                else
                {
                    this.Builder.Append('.');
                    this.AppendIdentifier(property);
                }
            }

            if (property.SetMethod == null)
            {
                this.Builder.Append(" => ");
                this.AppendMemberReference(items[0], scope);
                AppendDefinition(property);
                this.Builder.AppendLine(';');
            }
            else
            {
                this.Builder.AppendLine();
                this.Builder.AppendIndentation();
                this.Builder.AppendLine('{');
                this.Builder.IncrementIndentation();
                try
                {
                    if (items.Count == 1)
                    {
                        IMemberInfo item = items[0];

                        if (property.GetMethod is IMethodSymbol)
                        {
                            this.Builder.AppendIndentation();
                            this.Builder.Append("get => ");
                            this.AppendMemberReference(item, scope);
                            AppendDefinition(property);
                            this.Builder.AppendLine(';');
                        }
                        if (property.SetMethod is IMethodSymbol)
                        {
                            this.Builder.AppendIndentation();
                            this.Builder.Append("set => ");
                            this.AppendMemberReference(item, scope);
                            AppendDefinition(property);
                            this.Builder.AppendLine(" = value;");
                        }
                    }
                    else
                    {
                        if (property.GetMethod is IMethodSymbol)
                        {
                            this.Builder.AppendIndentation();
                            this.Builder.Append("get => ");
                            this.AppendMemberReference(items.Last(), scope);
                            AppendDefinition(property);
                            this.Builder.AppendLine(';');
                        }
                        if (property.SetMethod is IMethodSymbol)
                        {
                            this.Builder.AppendIndentation();
                            this.Builder.AppendLine("set");
                            this.Builder.AppendIndentation();
                            this.Builder.AppendLine('{');
                            foreach (IMemberInfo item in items)
                            {
                                this.Builder.IncrementIndentation();
                                try
                                {
                                    this.Builder.AppendIndentation();
                                    this.AppendMemberReference(item, scope);
                                    AppendDefinition(property);
                                    this.Builder.AppendLine(" = value;");
                                }
                                finally
                                {
                                    this.Builder.DecrementIndentation();
                                }
                            }
                            this.Builder.AppendIndentation();
                            this.Builder.AppendLine('}');
                        }
                    }
                }
                finally
                {
                    this.Builder.DecrementIndentation();
                }
                this.Builder.AppendIndentation();
                this.Builder.AppendLine('}');
            }
        }

        public void AppendEventDefinition(IEventSymbol @event, ScopeInfo scope, INamedTypeSymbol @interface, List<IMemberInfo> items)
        {
            this.Builder.AppendIndentation();
            this.Builder.Append("event");
            this.Builder.Append(' ');
            this.AppendTypeReference(@event.Type, scope);
            this.Builder.Append(' ');
            this.AppendTypeReference(@interface, scope);
            this.Builder.Append('.');
            this.AppendIdentifier(@event);
            this.Builder.AppendLine();
            this.Builder.AppendIndentation();
            this.Builder.AppendLine('{');
            this.Builder.IncrementIndentation();
            try
            {
                if (items.Count == 1)
                {
                    IMemberInfo item = items[0];
                    this.Builder.AppendIndentation();
                    this.Builder.Append("add => ");
                    this.AppendMemberReference(item, scope);
                    this.Builder.Append('.');
                    this.AppendIdentifier(@event);
                    this.Builder.AppendLine(" += value;");
                    this.Builder.AppendIndentation();
                    this.Builder.Append("remove => ");
                    this.AppendMemberReference(item, scope);
                    this.Builder.Append('.');
                    this.AppendIdentifier(@event);
                    this.Builder.AppendLine(" -= value;");
                }
                else
                {
                    this.Builder.AppendIndentation();
                    this.Builder.AppendLine("add");
                    this.Builder.AppendIndentation();
                    this.Builder.AppendLine('{');
                    foreach (IMemberInfo item in items)
                    {
                        this.Builder.IncrementIndentation();
                        try
                        {
                            this.Builder.AppendIndentation();
                            this.AppendMemberReference(item, scope);
                            this.Builder.Append('.');
                            this.AppendIdentifier(@event);
                            this.Builder.AppendLine(" += value;");
                        }
                        finally
                        {
                            this.Builder.DecrementIndentation();
                        }
                    }
                    this.Builder.AppendIndentation();
                    this.Builder.AppendLine('}');
                    this.Builder.AppendIndentation();
                    this.Builder.AppendLine("remove");
                    this.Builder.AppendIndentation();
                    this.Builder.AppendLine('{');
                    foreach (IMemberInfo item in items)
                    {
                        this.Builder.IncrementIndentation();
                        try
                        {
                            this.Builder.AppendIndentation();
                            this.AppendMemberReference(item, scope);
                            this.Builder.Append('.');
                            this.AppendIdentifier(@event);
                            this.Builder.AppendLine(" -= value;");
                        }
                        finally
                        {
                            this.Builder.DecrementIndentation();
                        }
                    }
                    this.Builder.AppendIndentation();
                    this.Builder.AppendLine('}');
                }
            }
            finally
            {
                this.Builder.DecrementIndentation();
            }
            this.Builder.AppendIndentation();
            this.Builder.AppendLine('}');
        }

        public void AppendTypeDeclarationBeginning(INamedTypeSymbol type, ScopeInfo scope)
        {
            this.Builder.Append("partial");
            this.Builder.Append(' ');
            if (type.TypeKind == TypeKind.Class)
            {
                bool isRecord = type.DeclaringSyntaxReferences.Any(i => i.GetSyntax() is RecordDeclarationSyntax);
                this.Builder.Append(isRecord ? "record" : "class");
            }
            else if (type.TypeKind == TypeKind.Struct)
            {
                this.Builder.Append("struct");
            }
            else if (type.TypeKind == TypeKind.Interface)
            {
                this.Builder.Append("interface");
            }
            else
            {
                throw new NotSupportedException();
            }
            this.Builder.Append(' ');
            this.AppendTypeReference(type, scope);
        }

        public void AppendNamespaceBeginning(string @namespace)
        {
            this.Builder.AppendIndentation();
            this.Builder.Append("namespace");
            this.Builder.Append(' ');
            this.Builder.AppendLine(@namespace);
            this.Builder.AppendIndentation();
            this.Builder.AppendLine('{');
            this.Builder.IncrementIndentation();
        }

        public void AppendHolderReference(ISymbol member, ScopeInfo scope)
        {
            if (member.IsStatic)
            {
                this.AppendTypeReference(member.ContainingType, scope);
            }
            else
            {
                this.Builder.Append("this");
            }
        }

        public void AppendMemberReference(IMemberInfo item, ScopeInfo scope)
        {
            if (item.CastRequired)
            {
                this.Builder.Append("((");
                this.AppendTypeReference(item.InterfaceType, scope);
                this.Builder.Append(')');
                this.AppendHolderReference(item.Member, scope);
                this.Builder.Append('.');
                this.AppendIdentifier(item.Member);
                this.Builder.Append(')');
            }
            else
            {
                this.AppendHolderReference(item.Member, scope);
                this.Builder.Append('.');
                this.AppendIdentifier(item.Member);
            }
        }

        public void AppendMethodCall(IMemberInfo item, IMethodSymbol method, ScopeInfo scope, bool async)
        {
            if (async)
            {
                this.Builder.Append("await");
                this.Builder.Append(' ');
            }
            this.AppendMemberReference(item, scope);
            this.Builder.Append('.');
            this.AppendIdentifier(method);
            if (method.IsGenericMethod)
            {
                this.Builder.Append('<');
                this.AppendTypeArgumentsCall(method.TypeArguments, scope);
                this.Builder.Append('>');
            }
            this.Builder.Append('(');
            this.AppendCallParameters(method.Parameters);
            this.Builder.Append(")");
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

        #region helper members

        private void AppendIdentifier(ISymbol symbol)
        {
            this.Builder.Append(this.GetSourceIdentifier(symbol));
        }

        private void AppendRefKind(RefKind kind)
        {
            switch (kind)
            {
                default: throw new NotSupportedException();
                case RefKind.None: break;
                case RefKind.In: this.Builder.Append("in"); break;
                case RefKind.Out: this.Builder.Append("out"); break;
                case RefKind.Ref: this.Builder.Append("ref"); break;
            }
        }

        private bool IsVerbatim(ISymbol symbol)
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

        #endregion
    }
}