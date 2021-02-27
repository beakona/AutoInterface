using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class CSharpCodeTextWriter : ICodeTextWriter
    {
        public CSharpCodeTextWriter(Compilation compilation)
        {
            this.Compilation = compilation;
        }

        public Compilation Compilation { get; }

        public void WriteTypeReference(SourceBuilder builder, ITypeSymbol type, ScopeInfo scope)
        {
            if (scope.TryGetAlias(type, out string? typeName))
            {
                if (typeName != null)
                {
                    builder.Append(typeName);
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
                        case SpecialType.System_Object: builder.Append("object"); break;
                        case SpecialType.System_Void: builder.Append("void"); break;
                        case SpecialType.System_Boolean: builder.Append("bool"); break;
                        case SpecialType.System_Char: builder.Append("char"); break;
                        case SpecialType.System_SByte: builder.Append("sbyte"); break;
                        case SpecialType.System_Byte: builder.Append("byte"); break;
                        case SpecialType.System_Int16: builder.Append("short"); break;
                        case SpecialType.System_UInt16: builder.Append("ushort"); break;
                        case SpecialType.System_Int32: builder.Append("int"); break;
                        case SpecialType.System_UInt32: builder.Append("uint"); break;
                        case SpecialType.System_Int64: builder.Append("long"); break;
                        case SpecialType.System_UInt64: builder.Append("ulong"); break;
                        case SpecialType.System_Decimal: builder.Append("decimal"); break;
                        case SpecialType.System_Single: builder.Append("float"); break;
                        case SpecialType.System_Double: builder.Append("double"); break;
                        //case SpecialType.System_Half: builder.Append("half"); break;
                        case SpecialType.System_String: builder.Append("string"); break;
                    }
                }

                if (processed == false)
                {
                    if (type is IArrayTypeSymbol array)
                    {
                        this.WriteTypeReference(builder, array.ElementType, scope);
                        builder.Append('[');
                        for (int i = 1; i < array.Rank; i++)
                        {
                            builder.Append(',');
                        }
                        builder.Append(']');
                    }
                    else
                    {
                        static bool IsTupleWithAliases(INamedTypeSymbol tuple)
                        {
                            return tuple.TupleElements.Any(i => i.CorrespondingTupleField != null && i.Equals(i.CorrespondingTupleField, SymbolEqualityComparer.Default) == false);
                        }

                        if (type.IsTupleType && type is INamedTypeSymbol tupleType && IsTupleWithAliases(tupleType))
                        {
                            builder.Append('(');
                            bool first = true;
                            foreach (IFieldSymbol field in tupleType.TupleElements)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    builder.Append(", ");
                                }
                                this.WriteTypeReference(builder, field.Type, scope);
                                builder.Append(' ');
                                this.WriteIdentifier(builder, field);
                            }
                            builder.Append(')');
                        }
                        else if (type is INamedTypeSymbol nt && SemanticFacts.IsNullableT(this.Compilation, nt))
                        {
                            this.WriteTypeReference(builder, nt.TypeArguments[0], scope);
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
                                        builder.Append(alias);
                                        builder.Append("::");
                                        builder.RegisterAlias(alias);
                                    }

                                    foreach (ISymbol symbol in symbols)
                                    {
                                        this.WriteIdentifier(builder, symbol);

                                        if (symbol is INamedTypeSymbol snt && snt.IsGenericType)
                                        {
                                            builder.Append('<');
                                            this.WriteTypeArgumentsDefinition(builder, snt.TypeArguments, scope);
                                            builder.Append('>');
                                        }

                                        builder.Append('.');
                                    }
                                }
                            }

                            this.WriteIdentifier(builder, type);

                            {
                                if (type is INamedTypeSymbol tnt && tnt.IsGenericType)
                                {
                                    builder.Append('<');
                                    this.WriteTypeArgumentsDefinition(builder, tnt.TypeArguments, scope);
                                    builder.Append('>');
                                }
                            }
                        }
                    }
                }
            }

            if (type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                builder.Append('?');
            }
        }

        public void WriteTypeArgumentsCall(SourceBuilder builder, IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope)
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
                    builder.Append(", ");
                }

                if (scope.TryGetAlias(t, out string? alias))
                {
                    if (alias != null)
                    {
                        builder.Append(alias);
                    }
                }
                else
                {
                    this.WriteIdentifier(builder, t);
                }
            }
        }

        public void WriteTypeArgumentsDefinition(SourceBuilder builder, IEnumerable<ITypeSymbol> typeArguments, ScopeInfo scope)
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
                    builder.Append(", ");
                }
                this.WriteTypeReference(builder, t, scope);
            }
        }

        public void WriteParameterDefinition(SourceBuilder builder, ScopeInfo scope, IEnumerable<IParameterSymbol> parameters)
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
                    builder.Append(", ");
                }

                if (parameter.IsParams)
                {
                    builder.Append("params");
                    builder.Append(' ');
                }
                else
                {
                    this.WriteRefKind(builder, parameter.RefKind);
                    builder.AppendSpaceIfNeccessary();
                }

                this.WriteTypeReference(builder, parameter.Type, scope);
                builder.Append(' ');
                this.WriteIdentifier(builder, parameter);
                //if (parameter.HasExplicitDefaultValue)
                //{
                //    builder.Append(" = ");
                //    builder.Append(parameter.ExplicitDefaultValue ?? "default");
                //}
            }
        }

        public void WriteCallParameters(SourceBuilder builder, IEnumerable<IParameterSymbol> parameters)
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
                    builder.Append(", ");
                }
                this.WriteRefKind(builder, parameter.RefKind);
                builder.AppendSpaceIfNeccessary();
                this.WriteIdentifier(builder, parameter);
            }
        }

        public void WriteMethodDefinition(SourceBuilder builder, IMethodSymbol method, ScopeInfo scope, INamedTypeSymbol @interface, IEnumerable<IMemberInfo> references)
        {
            ScopeInfo methodScope = new ScopeInfo(scope);

            if (method.IsGenericMethod)
            {
                methodScope.CreateAliases(method.TypeArguments);
            }

            (bool isAsync, bool methodReturnsValue) = SemanticFacts.IsAsyncAndGetReturnType(this.Compilation, method);

            int rcount = references.Count();
            bool useAsync = rcount > 1;
            bool returnsValue = (isAsync && methodReturnsValue == false) ? useAsync == false : methodReturnsValue;

            builder.AppendIndentation();
            if (isAsync && useAsync)
            {
                builder.Append("async");
                builder.Append(' ');
            }
            this.WriteTypeReference(builder, method.ReturnType, methodScope);
            builder.Append(' ');
            this.WriteTypeReference(builder, @interface, scope);
            builder.Append('.');
            this.WriteIdentifier(builder, method);
            if (method.IsGenericMethod)
            {
                builder.Append('<');
                this.WriteTypeArgumentsCall(builder, method.TypeArguments, methodScope);
                builder.Append('>');
            }
            builder.Append('(');
            this.WriteParameterDefinition(builder, methodScope, method.Parameters);
            builder.Append(")");

            if (rcount == 1)
            {
                builder.Append(" => ");
                this.WriteMethodCall(builder, references.First(), method, methodScope, isAsync && useAsync, false, false);
                builder.AppendLine(';');
            }
            else
            {
                builder.AppendLine();
                builder.AppendIndentation();
                builder.AppendLine('{');
                builder.IncrementIndentation();
                try
                {
                    if (isAsync && useAsync)
                    {
                        {
                            int index = 0;
                            foreach (IMemberInfo reference in references)
                            {
                                builder.AppendIndentation();
                                builder.Append("var temp");
                                builder.Append(index);
                                builder.Append(" = ");
                                this.WriteMethodCall(builder, reference, method, methodScope, false, false, false);
                                builder.Append(".ConfigureAwait(false)");
                                builder.AppendLine(';');
                                index++;
                            }
                        }
                        {
                            int index = 0;
                            foreach (IMemberInfo reference in references)
                            {
                                bool last = index + 1 == rcount;

                                builder.AppendIndentation();
                                if (returnsValue && last)
                                {
                                    builder.Append("return");
                                    builder.Append(' ');
                                }

                                builder.Append("await temp");
                                builder.Append(index);
                                builder.AppendLine(';');
                                index++;
                            }
                        }
                    }
                    else
                    {
                        int index = 0;
                        foreach (IMemberInfo reference in references)
                        {
                            bool last = index + 1 == rcount;

                            builder.AppendIndentation();
                            if (returnsValue && last)
                            {
                                builder.Append("return");
                                builder.Append(' ');
                            }

                            bool async = isAsync && useAsync;
                            this.WriteMethodCall(builder, reference, method, methodScope, async, false, false);
                            builder.AppendLine(';');
                            index++;
                        }
                    }
                }
                finally
                {
                    builder.DecrementIndentation();
                }
                builder.AppendIndentation();
                builder.AppendLine('}');
            }
        }

        public void WritePropertyDefinition(SourceBuilder builder, IPropertySymbol property, ScopeInfo scope, INamedTypeSymbol @interface, IEnumerable<IMemberInfo> references)
        {
            builder.AppendIndentation();
            this.WriteTypeReference(builder, property.Type, scope);
            builder.Append(' ');
            this.WriteTypeReference(builder, @interface, scope);
            builder.Append('.');

            if (property.IsIndexer)
            {
                builder.Append("this[");
                this.WriteParameterDefinition(builder, scope, property.Parameters);
                builder.Append(']');
            }
            else
            {
                this.WriteIdentifier(builder, property);
            }

            void AppendDefinition(IPropertySymbol property)
            {
                if (property.IsIndexer)
                {
                    builder.Append('[');
                    this.WriteCallParameters(builder, property.Parameters);
                    builder.Append(']');
                }
                else
                {
                    builder.Append('.');
                    this.WriteIdentifier(builder, property);
                }
            }

            if (property.SetMethod == null)
            {
                builder.Append(" => ");
                this.WriteMemberReference(builder, references.First(), scope, SemanticFacts.IsNullable(this.Compilation, property.Type), true);
                AppendDefinition(property);
                builder.AppendLine(';');
            }
            else
            {
                builder.AppendLine();
                builder.AppendIndentation();
                builder.AppendLine('{');
                builder.IncrementIndentation();
                try
                {
                    if (references.Count() == 1)
                    {
                        IMemberInfo reference = references.First();

                        if (property.GetMethod is IMethodSymbol)
                        {
                            builder.AppendIndentation();
                            builder.Append("get => ");
                            this.WriteMemberReference(builder, reference, scope, SemanticFacts.IsNullable(this.Compilation, property.Type), true);
                            AppendDefinition(property);
                            builder.AppendLine(';');
                        }
                        if (property.SetMethod is IMethodSymbol)
                        {
                            builder.AppendIndentation();
                            builder.Append("set => ");
                            this.WriteMemberReference(builder, reference, scope, false, false);
                            AppendDefinition(property);
                            builder.AppendLine(" = value;");
                        }
                    }
                    else
                    {
                        if (property.GetMethod is IMethodSymbol)
                        {
                            builder.AppendIndentation();
                            builder.Append("get => ");
                            this.WriteMemberReference(builder, references.Last(), scope, SemanticFacts.IsNullable(this.Compilation, property.Type), true);
                            AppendDefinition(property);
                            builder.AppendLine(';');
                        }
                        if (property.SetMethod is IMethodSymbol)
                        {
                            builder.AppendIndentation();
                            builder.AppendLine("set");
                            builder.AppendIndentation();
                            builder.AppendLine('{');
                            builder.IncrementIndentation();
                            try
                            {
                                foreach (IMemberInfo reference in references)
                                {
                                    builder.AppendIndentation();
                                    this.WriteMemberReference(builder, reference, scope, false, false);
                                    AppendDefinition(property);
                                    builder.AppendLine(" = value;");
                                }
                            }
                            finally
                            {
                                builder.DecrementIndentation();
                            }
                            builder.AppendIndentation();
                            builder.AppendLine('}');
                        }
                    }
                }
                finally
                {
                    builder.DecrementIndentation();
                }
                builder.AppendIndentation();
                builder.AppendLine('}');
            }
        }

        public void WriteEventDefinition(SourceBuilder builder, IEventSymbol @event, ScopeInfo scope, INamedTypeSymbol @interface, IEnumerable<IMemberInfo> references)
        {
            builder.AppendIndentation();
            builder.Append("event");
            builder.Append(' ');
            this.WriteTypeReference(builder, @event.Type, scope);
            builder.Append(' ');
            this.WriteTypeReference(builder, @interface, scope);
            builder.Append('.');
            this.WriteIdentifier(builder, @event);
            builder.AppendLine();
            builder.AppendIndentation();
            builder.AppendLine('{');
            builder.IncrementIndentation();
            try
            {
                if (references.Count() == 1)
                {
                    IMemberInfo reference = references.First();
                    builder.AppendIndentation();
                    builder.Append("add => ");
                    this.WriteMemberReference(builder, reference, scope, false, false);
                    builder.Append('.');
                    this.WriteIdentifier(builder, @event);
                    builder.AppendLine(" += value;");
                    builder.AppendIndentation();
                    builder.Append("remove => ");
                    this.WriteMemberReference(builder, reference, scope, false, false);
                    builder.Append('.');
                    this.WriteIdentifier(builder, @event);
                    builder.AppendLine(" -= value;");
                }
                else
                {
                    builder.AppendIndentation();
                    builder.AppendLine("add");
                    builder.AppendIndentation();
                    builder.AppendLine('{');
                    builder.IncrementIndentation();
                    try
                    {
                        foreach (IMemberInfo reference in references)
                        {
                            builder.AppendIndentation();
                            this.WriteMemberReference(builder, reference, scope, false, false);
                            builder.Append('.');
                            this.WriteIdentifier(builder, @event);
                            builder.AppendLine(" += value;");
                        }
                    }
                    finally
                    {
                        builder.DecrementIndentation();
                    }
                    builder.AppendIndentation();
                    builder.AppendLine('}');
                    builder.AppendIndentation();
                    builder.AppendLine("remove");
                    builder.AppendIndentation();
                    builder.AppendLine('{');
                    builder.IncrementIndentation();
                    try
                    {
                        foreach(IMemberInfo reference in references)
                        {
                            builder.AppendIndentation();
                            this.WriteMemberReference(builder, reference, scope, false, false);
                            builder.Append('.');
                            this.WriteIdentifier(builder, @event);
                            builder.AppendLine(" -= value;");
                        }
                    }
                    finally
                    {
                        builder.DecrementIndentation();
                    }
                    builder.AppendIndentation();
                    builder.AppendLine('}');
                }
            }
            finally
            {
                builder.DecrementIndentation();
            }
            builder.AppendIndentation();
            builder.AppendLine('}');
        }

        public void WriteTypeDeclarationBeginning(SourceBuilder builder, INamedTypeSymbol type, ScopeInfo scope)
        {
            builder.Append("partial");
            builder.Append(' ');
            if (type.TypeKind == TypeKind.Class)
            {
                bool isRecord = type.DeclaringSyntaxReferences.Any(i => i.GetSyntax() is RecordDeclarationSyntax);
                builder.Append(isRecord ? "record" : "class");
            }
            else if (type.TypeKind == TypeKind.Struct)
            {
                builder.Append("struct");
            }
            else if (type.TypeKind == TypeKind.Interface)
            {
                builder.Append("interface");
            }
            else
            {
                throw new NotSupportedException(nameof(WriteTypeDeclarationBeginning));
            }
            builder.Append(' ');
            this.WriteTypeReference(builder, type, scope);
        }

        public void WriteNamespaceBeginning(SourceBuilder builder, IEnumerable<INamespaceSymbol> @namespace)
        {
            builder.AppendIndentation();
            builder.Append("namespace");
            builder.Append(' ');
            builder.AppendLine(string.Join(".", @namespace.Select(i => this.GetSourceIdentifier(i))));
            builder.AppendIndentation();
            builder.AppendLine('{');
            builder.IncrementIndentation();
        }

        public void WriteHolderReference(SourceBuilder builder, ISymbol member, ScopeInfo scope)
        {
            if (member.IsStatic)
            {
                this.WriteTypeReference(builder, member.ContainingType, scope);
            }
            else
            {
                builder.Append("this");
            }
        }

        public void WriteMemberReference(SourceBuilder builder, IMemberInfo item, ScopeInfo scope, bool typeIsNullable, bool allowCoalescing)
        {
            bool expressionIsNullable;
            if (item.CastRequired)
            {
                builder.Append('(');
                this.WriteHolderReference(builder, item.Member, scope);
                builder.Append('.');
                this.WriteIdentifier(builder, item.Member);
                builder.Append(" as ");
                this.WriteTypeReference(builder, item.InterfaceType.WithNullableAnnotation(NullableAnnotation.NotAnnotated), scope);
                builder.Append(')');

                expressionIsNullable = true;
            }
            else
            {
                this.WriteHolderReference(builder, item.Member, scope);
                builder.Append('.');
                this.WriteIdentifier(builder, item.Member);

                expressionIsNullable = SemanticFacts.IsNullable(this.Compilation, item.ReceiverType);
            }

            if (allowCoalescing)
            {
                if (expressionIsNullable)
                {
                    builder.Append('!');
                }
                else
                {
                    if (typeIsNullable)
                    {
                        //builder.Append('A');
                    }
                    else
                    {
                        //builder.Append('B');
                    }
                }
            }
            else
            {
                if (expressionIsNullable)
                {
                    if (typeIsNullable)
                    {
                        //builder.Append('C');
                    }
                    else
                    {
                        builder.Append('!');
                    }
                }
                else
                {
                    if (typeIsNullable)
                    {
                        //builder.Append('D');
                    }
                }
            }
        }

        public void WriteMethodCall(SourceBuilder builder, IMemberInfo item, IMethodSymbol method, ScopeInfo scope, bool async, bool typeIsNullable, bool allowCoalescing)
        {
            if (async)
            {
                builder.Append("await");
                builder.Append(' ');
            }
            this.WriteMemberReference(builder, item, scope, typeIsNullable, allowCoalescing);
            builder.Append('.');
            this.WriteIdentifier(builder, method);
            if (method.IsGenericMethod)
            {
                builder.Append('<');
                this.WriteTypeArgumentsCall(builder, method.TypeArguments, scope);
                builder.Append('>');
            }
            builder.Append('(');
            this.WriteCallParameters(builder, method.Parameters);
            builder.Append(")");
        }

        public void WriteIdentifier(SourceBuilder builder, ISymbol symbol)
        {
            builder.Append(this.GetSourceIdentifier(symbol));
        }

        public void WriteRefKind(SourceBuilder builder, RefKind kind)
        {
            switch (kind)
            {
                default: throw new NotSupportedException(nameof(WriteRefKind));
                case RefKind.None: break;
                case RefKind.In: builder.Append("in"); break;
                case RefKind.Out: builder.Append("out"); break;
                case RefKind.Ref: builder.Append("ref"); break;
            }
        }

        #region helper members

        private string GetSourceIdentifier(ISymbol symbol)
        {
            if (symbol is IPropertySymbol property && property.IsIndexer)
            {
                return "this";
            }
            else if (this.IsVerbatim(symbol))
            {
                return $"@{symbol.Name}";
            }
            else
            {
                return symbol.Name;
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
                else if (syntax is IndexerDeclarationSyntax)
                {
                    throw new InvalidOperationException("trying to resolve indexer name");
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