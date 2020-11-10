using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class CSharpSourceTextBuilder : ISourceTextBuilder
    {
        public CSharpSourceTextBuilder(CSharpBuildContext context)
        {
            this.Context = context;
        }

        IBuildContext ISourceTextBuilder.Context => this.Context;
        public CSharpBuildContext Context { get; }

        private readonly StringBuilder output = new StringBuilder();
        private readonly HashSet<string> aliases = new HashSet<string>();

        public override string ToString()
        {
            if (this.aliases.Count == 0)
            {
                return output.ToString();
            }

            StringBuilder result = new StringBuilder();
            foreach (string alias in this.aliases.OrderByDescending(i => i))
            {
                result.Append("extern alias ");
                result.AppendLine(alias);
            }
            result.AppendLine();
            result.Append(this.output);

            return result.ToString();
        }

        public void AppendLine() => this.output.AppendLine();

        public void AppendLine(string? text)
        {
            if (text != null)
            {
                this.output.AppendLine(text);
            }
            else
            {
                this.output.AppendLine();
            }
        }

        public void Append(string text)
        {
            if (text != null)
            {
                this.output.Append(text);
            }
        }

        public void Append(object? value)
        {
            if (value != null)
            {
                this.output.Append(value);
            }
        }

        public void Append(char c) => this.output.Append(c);

        private string indentation = "";

        public void IncrementIndentation() => this.indentation += '\t';

        public void DecrementIndentation()
        {
            if (this.indentation.Length > 1)
            {
                this.indentation = this.indentation.Substring(0, this.indentation.Length - 1);
            }
            else
            {
                this.indentation = "";
            }
        }

        public void AppendIndentation()
        {
            this.Append(this.indentation);
        }

        public void AppendIdentifier(ISymbol symbol)
        {
            this.Append(this.Context.GetSourceIdentifier(symbol));
        }

        public void AppendTypeReference(ITypeSymbol type, ScopeInfo scope)
        {
            if (scope.TryGetAlias(type, out string? typeName))
            {
                if (typeName != null)
                {
                    this.Append(typeName);
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
                        case SpecialType.System_Object: this.Append("object"); break;
                        case SpecialType.System_Void: this.Append("void"); break;
                        case SpecialType.System_Boolean: this.Append("bool"); break;
                        case SpecialType.System_Char: this.Append("char"); break;
                        case SpecialType.System_SByte: this.Append("sbyte"); break;
                        case SpecialType.System_Byte: this.Append("byte"); break;
                        case SpecialType.System_Int16: this.Append("short"); break;
                        case SpecialType.System_UInt16: this.Append("ushort"); break;
                        case SpecialType.System_Int32: this.Append("int"); break;
                        case SpecialType.System_UInt32: this.Append("uint"); break;
                        case SpecialType.System_Int64: this.Append("long"); break;
                        case SpecialType.System_UInt64: this.Append("ulong"); break;
                        case SpecialType.System_Decimal: this.Append("decimal"); break;
                        case SpecialType.System_Single: this.Append("float"); break;
                        case SpecialType.System_Double: this.Append("double"); break;
                        //case SpecialType.System_Half: this.Append("half"); break;
                        case SpecialType.System_String: this.Append("string"); break;
                    }
                }

                if (processed == false)
                {
                    if (type is IArrayTypeSymbol array)
                    {
                        this.AppendTypeReference(array.ElementType, scope);
                        this.Append('[');
                        for (int i = 1; i < array.Rank; i++)
                        {
                            this.Append(',');
                        }
                        this.Append(']');
                    }
                    else
                    {
                        static bool IsTupleWithAliases(INamedTypeSymbol tuple)
                        {
                            return tuple.TupleElements.Any(i => i.CorrespondingTupleField != null && i.Equals(i.CorrespondingTupleField, SymbolEqualityComparer.Default) == false);
                        }

                        if (type.IsTupleType && type is INamedTypeSymbol tupleType && IsTupleWithAliases(tupleType))
                        {
                            this.Append('(');
                            bool first = true;
                            foreach (IFieldSymbol field in tupleType.TupleElements)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    this.Append(", ");
                                }
                                this.AppendTypeReference(field.Type, scope);
                                this.Append(' ');
                                this.AppendIdentifier(field);
                            }
                            this.Append(')');
                        }
                        else if (type is INamedTypeSymbol nt && this.Context.IsNullableT(nt))
                        {
                            this.AppendTypeReference(nt.TypeArguments[0], scope);
                        }
                        else
                        {
                            if (type is ITypeParameterSymbol == false)
                            {
                                if (type.Equals(scope.Type, SymbolEqualityComparer.Default) == false)
                                {
                                    string? alias = SemanticFacts.ResolveAssemblyAlias(scope.Compilation, type.ContainingAssembly);
                                    ISymbol[] symbols;
                                    if (alias == null)
                                    {
                                        symbols = SemanticFacts.GetRelativeSymbols(type, scope.Type);
                                    }
                                    else
                                    {
                                        symbols = SemanticFacts.GetContainingSymbols(type, false);
                                        this.Append(alias);
                                        this.Append("::");
                                        this.aliases.Add(alias);
                                    }

                                    foreach (ISymbol symbol in symbols)
                                    {
                                        this.AppendIdentifier(symbol);

                                        if (symbol is INamedTypeSymbol snt && snt.IsGenericType)
                                        {
                                            this.Append('<');
                                            this.AppendTypeArgumentsDefinition(snt.TypeArguments, scope);
                                            this.Append('>');
                                        }

                                        this.Append('.');
                                    }
                                }
                            }

                            this.AppendIdentifier(type);

                            {
                                if (type is INamedTypeSymbol tnt && tnt.IsGenericType)
                                {
                                    this.Append('<');
                                    this.AppendTypeArgumentsDefinition(tnt.TypeArguments, scope);
                                    this.Append('>');
                                }
                            }
                        }
                    }
                }
            }

            if (type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                this.Append('?');
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
                    this.Append(", ");
                }

                if (scope.TryGetAlias(t, out string? alias))
                {
                    if (alias != null)
                    {
                        this.Append(alias);
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
                    this.Append(", ");
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
                    this.Append(", ");
                }
                this.Append(CSharpSourceTextBuilder.AppendRefKind(parameter.RefKind));
                this.AppendIdentifier(parameter);
            }
        }

        public void AppendMethodDefinition(Compilation compilation, IMethodSymbol method, ScopeInfo scope, INamedTypeSymbol @interface, List<AutoInterfaceInfo> items)
        {
            ScopeInfo methodScope = new ScopeInfo(scope);

            bool useAsync = items.Count > 1;

            bool isAsync = false;
            bool returnsValue = false;
            if (method.ReturnType is INamedTypeSymbol returnType)
            {
                if (returnType.IsGenericType)
                {
                    if (returnType.IsUnboundGenericType == false)
                    {
                        returnType = returnType.ConstructUnboundGenericType();
                    }
                    INamedTypeSymbol? symbolTask1 = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")?.ConstructUnboundGenericType();
                    INamedTypeSymbol? symbolValueTask1 = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1")?.ConstructUnboundGenericType();
                    if (symbolTask1 != null && returnType.Equals(symbolTask1, SymbolEqualityComparer.Default) || symbolValueTask1 != null && returnType.Equals(symbolValueTask1, SymbolEqualityComparer.Default))
                    {
                        isAsync = true;
                        returnsValue = true;
                    }
                }
                else
                {
                    INamedTypeSymbol? symbolTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                    INamedTypeSymbol? symbolValueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
                    if (symbolTask != null && returnType.Equals(symbolTask, SymbolEqualityComparer.Default) || symbolValueTask != null && returnType.Equals(symbolValueTask, SymbolEqualityComparer.Default))
                    {
                        isAsync = true;
                        returnsValue = useAsync == false;
                    }
                }
            }
            if (isAsync == false)
            {
                returnsValue = method.ReturnsVoid == false;
            }

            if (method.IsGenericMethod)
            {
                methodScope.CreateAliases(method.TypeArguments);
            }

            this.AppendIndentation();
            if (isAsync && useAsync)
            {
                this.Append("async ");
            }
            this.AppendTypeReference(method.ReturnType, methodScope);
            this.Append(' ');
            this.AppendTypeReference(@interface, scope);
            this.Append('.');
            this.AppendIdentifier(method);
            if (method.IsGenericMethod)
            {
                this.Append('<');
                this.AppendTypeArgumentsCall(method.TypeArguments, methodScope);
                this.Append('>');
            }
            this.Append('(');
            bool first = true;
            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    this.Append(", ");
                }

                if (parameter.IsParams)
                {
                    this.Append("params ");
                }
                this.Append(CSharpSourceTextBuilder.AppendRefKind(parameter.RefKind));
                this.AppendTypeReference(parameter.Type, methodScope);
                this.Append(' ');
                this.AppendIdentifier(parameter);
                if (parameter.HasExplicitDefaultValue)
                {
                    this.Append(" = ");
                    this.Append(parameter.ExplicitDefaultValue ?? "null");
                }
            }
            this.Append(")");

            if (items.Count == 1)
            {
                this.Append(" => ");
                this.AppendMethodCall(items[0], method, methodScope, isAsync && useAsync);
                this.AppendLine();
            }
            else
            {
                this.AppendLine();
                this.AppendIndentation();
                this.AppendLine("{");
                for (int m = 0; m < items.Count; m++)
                {
                    this.IncrementIndentation();
                    try
                    {
                        this.AppendIndentation();
                        if (returnsValue && m + 1 == items.Count)
                        {
                            this.Append("return ");
                        }
                        this.AppendMethodCall(items[m], method, methodScope, isAsync && useAsync);
                        this.AppendLine();
                    }
                    finally
                    {
                        this.DecrementIndentation();
                    }
                }
                this.AppendIndentation();
                this.AppendLine("}");
            }
        }

        public void AppendPropertyDefinition(IPropertySymbol property, ScopeInfo scope, INamedTypeSymbol @interface, List<AutoInterfaceInfo> items)
        {
            this.AppendIndentation();
            this.AppendTypeReference(property.Type, scope);
            this.Append(' ');
            this.AppendTypeReference(@interface, scope);
            this.Append('.');

            if (property.IsIndexer)
            {
                this.Append("this[");
                bool first = true;
                foreach (IParameterSymbol parameter in property.Parameters)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        this.Append(", ");
                    }
                    this.Append(CSharpSourceTextBuilder.AppendRefKind(parameter.RefKind));
                    this.AppendTypeReference(parameter.Type, scope);
                    this.Append(' ');
                    this.AppendIdentifier(parameter);
                }
                this.Append(']');
            }
            else
            {
                this.AppendIdentifier(property);
            }

            static void AppendDefinition(IPropertySymbol property, ISourceTextBuilder ei)
            {
                if (property.IsIndexer)
                {
                    ei.Append('[');
                    ei.AppendCallParameters(property.Parameters);
                    ei.Append(']');
                }
                else
                {
                    ei.Append('.');
                    ei.AppendIdentifier(property);
                }
            }

            if (property.SetMethod == null)
            {
                this.Append(" => ");
                this.AppendMemberReference(items[0], scope);
                AppendDefinition(property, this);
                this.AppendLine(";");
            }
            else
            {
                this.AppendLine();
                this.AppendIndentation();
                this.AppendLine("{");
                this.IncrementIndentation();
                try
                {
                    if (items.Count == 1)
                    {
                        AutoInterfaceInfo item = items[0];

                        if (property.GetMethod is IMethodSymbol)
                        {
                            this.AppendIndentation();
                            this.Append("get => ");
                            this.AppendMemberReference(item, scope);
                            AppendDefinition(property, this);
                            this.AppendLine(";");
                        }
                        if (property.SetMethod is IMethodSymbol)
                        {
                            this.AppendIndentation();
                            this.Append("set => ");
                            this.AppendMemberReference(item, scope);
                            AppendDefinition(property, this);
                            this.AppendLine(" = value;");
                        }
                    }
                    else
                    {
                        if (property.GetMethod is IMethodSymbol)
                        {
                            this.AppendIndentation();
                            this.Append("get => ");
                            this.AppendMemberReference(items.Last(), scope);
                            AppendDefinition(property, this);
                            this.AppendLine(";");
                        }
                        if (property.SetMethod is IMethodSymbol)
                        {
                            this.AppendIndentation();
                            this.AppendLine("set");
                            this.AppendIndentation();
                            this.AppendLine("{");
                            foreach (AutoInterfaceInfo item in items)
                            {
                                this.IncrementIndentation();
                                try
                                {
                                    this.AppendIndentation();
                                    this.AppendMemberReference(item, scope);
                                    AppendDefinition(property, this);
                                    this.AppendLine(" = value;");
                                }
                                finally
                                {
                                    this.DecrementIndentation();
                                }
                            }
                            this.AppendIndentation();
                            this.AppendLine("}");
                        }
                    }
                }
                finally
                {
                    this.DecrementIndentation();
                }
                this.AppendIndentation();
                this.AppendLine("}");
            }
        }

        public void AppendEventDefinition(IEventSymbol @event, ScopeInfo scope, INamedTypeSymbol @interface, List<AutoInterfaceInfo> items)
        {
            this.AppendIndentation();
            this.Append("event ");
            this.AppendTypeReference(@event.Type, scope);
            this.Append(' ');
            this.AppendTypeReference(@interface, scope);
            this.Append('.');
            this.AppendIdentifier(@event);
            this.AppendLine();
            this.AppendIndentation();
            this.AppendLine("{");
            this.IncrementIndentation();
            try
            {
                if (items.Count == 1)
                {
                    AutoInterfaceInfo item = items[0];
                    this.AppendIndentation();
                    this.Append("add => ");
                    this.AppendMemberReference(item, scope);
                    this.Append('.');
                    this.AppendIdentifier(@event);
                    this.AppendLine(" += value;");
                    this.AppendIndentation();
                    this.Append("remove => ");
                    this.AppendMemberReference(item, scope);
                    this.Append('.');
                    this.AppendIdentifier(@event);
                    this.AppendLine(" -= value;");
                }
                else
                {
                    this.AppendIndentation();
                    this.AppendLine("add");
                    this.AppendIndentation();
                    this.AppendLine("{");
                    foreach (AutoInterfaceInfo item in items)
                    {
                        this.IncrementIndentation();
                        try
                        {
                            this.AppendIndentation();
                            this.AppendMemberReference(item, scope);
                            this.Append('.');
                            this.AppendIdentifier(@event);
                            this.AppendLine(" += value;");
                        }
                        finally
                        {
                            this.DecrementIndentation();
                        }
                    }
                    this.AppendIndentation();
                    this.AppendLine("}");
                    this.AppendIndentation();
                    this.AppendLine("remove");
                    this.AppendIndentation();
                    this.AppendLine("{");
                    foreach (AutoInterfaceInfo item in items)
                    {
                        this.IncrementIndentation();
                        try
                        {
                            this.AppendIndentation();
                            this.AppendMemberReference(item, scope);
                            this.Append('.');
                            this.AppendIdentifier(@event);
                            this.AppendLine(" -= value;");
                        }
                        finally
                        {
                            this.DecrementIndentation();
                        }
                    }
                    this.AppendIndentation();
                    this.AppendLine("}");
                }
            }
            finally
            {
                this.DecrementIndentation();
            }
            this.AppendIndentation();
            this.AppendLine("}");
        }

        public void AppendTypeDeclarationBegin(INamedTypeSymbol type, ScopeInfo scope)
        {
            this.Append("partial ");
            if (type.TypeKind == TypeKind.Class)
            {
                bool isRecord = type.DeclaringSyntaxReferences.Any(i => i.GetSyntax() is RecordDeclarationSyntax);
                this.Append(isRecord ? "record" : "class");
            }
            else if (type.TypeKind == TypeKind.Struct)
            {
                this.Append("struct");
            }
            else if (type.TypeKind == TypeKind.Interface)
            {
                this.Append("interface");
            }
            else
            {
                throw new NotSupportedException();
            }
            this.Append(' ');
            this.AppendTypeReference(type, scope);
        }

        public void AppendNamespaceBegin(string @namespace)
        {
            this.AppendIndentation();
            this.Append("namespace ");
            this.AppendLine(@namespace);
            this.AppendIndentation();
            this.AppendLine("{");
            this.IncrementIndentation();
        }

        #region helper methods

        private void AppendHolderReference(ISymbol member, ScopeInfo scope)
        {
            if (member.IsStatic)
            {
                this.AppendTypeReference(member.ContainingType, scope);
            }
            else
            {
                this.Append("this");
            }
        }

        private void AppendMemberReference(AutoInterfaceInfo item, ScopeInfo scope)
        {
            if (item.CastRequired)
            {
                this.Append("((");
                this.AppendTypeReference(item.InterfaceType, scope);
                this.Append(')');
                this.AppendHolderReference(item.Member, scope);
                this.Append('.');
                this.AppendIdentifier(item.Member);
                this.Append(')');
            }
            else
            {
                this.AppendHolderReference(item.Member, scope);
                this.Append('.');
                this.AppendIdentifier(item.Member);
            }
        }

        private static string AppendRefKind(RefKind kind)
        {
            switch (kind)
            {
                default: throw new NotSupportedException();
                case RefKind.None: return "";
                case RefKind.In: return "in ";
                case RefKind.Out: return "out ";
                case RefKind.Ref: return "ref ";
            }
        }

        private void AppendMethodCall(AutoInterfaceInfo item, IMethodSymbol method, ScopeInfo scope, bool async)
        {
            if (async)
            {
                this.Append("await ");
            }
            this.AppendMemberReference(item, scope);
            this.Append('.');
            this.AppendIdentifier(method);
            if (method.IsGenericMethod)
            {
                this.Append('<');
                this.AppendTypeArgumentsCall(method.TypeArguments, scope);
                this.Append('>');
            }
            this.Append('(');
            this.AppendCallParameters(method.Parameters);
            this.Append(");");
        }

        #endregion helper methods
    }
}