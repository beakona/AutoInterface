using BeaKona.AutoInterfaceGenerator.Templates;
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
        public CSharpCodeTextWriter(GeneratorExecutionContext context, Compilation compilation)
        {
            this.Context = context;
            this.Compilation = compilation;
        }

        public GeneratorExecutionContext Context { get; }
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
            ScopeInfo methodScope = new(scope);

            if (method.IsGenericMethod)
            {
                methodScope.CreateAliases(method.TypeArguments);
            }

            (bool isAsync, bool methodReturnsValue) = SemanticFacts.IsAsyncAndGetReturnType(this.Compilation, method);

            PartialTemplate? template = this.GetMatchedTemplates(references, AutoInterfaceTargets.Method, method.Name);

            int rcount = references.Count();
            bool canUseAsync = template != null || rcount > 1;
            bool returnsValue = (isAsync && methodReturnsValue == false) ? canUseAsync == false : methodReturnsValue;

            builder.AppendIndentation();
            if (isAsync && canUseAsync && (template != null || rcount > 1))
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

            if (rcount == 1 && template == null)
            {
                builder.Append(" => ");
                this.WriteMethodCall(builder, references.First(), method, methodScope, false, SemanticFacts.IsNullable(this.Compilation, method.ReturnType), true);
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
                    if (template != null)
                    {
                        TemplatedSourceTextGenerator generator = new(template.Template);

                        PartialMethodModel model = new();
                        model.Load(this, builder, @interface, scope, references);
                        model.Load(this, builder, method, methodScope, references);

                        bool separatorRequired = false;
                        generator.Emit(this, builder, model, ref separatorRequired);
                        if (separatorRequired)
                        {
                            builder.AppendLine();
                        }
                    }
                    else
                    {
                        if (isAsync && canUseAsync)
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

                                this.WriteMethodCall(builder, reference, method, methodScope, false, SemanticFacts.IsNullable(this.Compilation, method.ReturnType), true);
                                builder.AppendLine(';');
                                index++;
                            }
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
            AutoInterfaceTargets getterTarget, setterTarget;
            if (property.IsIndexer)
            {
                getterTarget = AutoInterfaceTargets.IndexerGetter;
                setterTarget = AutoInterfaceTargets.IndexerSetter;
            }
            else
            {
                getterTarget = AutoInterfaceTargets.PropertyGetter;
                setterTarget = AutoInterfaceTargets.PropertySetter;
            }

            PartialTemplate? getterTemplate = this.GetMatchedTemplates(references, getterTarget, property.IsIndexer ? "this" : property.Name);
            PartialTemplate? setterTemplate = this.GetMatchedTemplates(references, setterTarget, property.IsIndexer ? "this" : property.Name);

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

            if (property.SetMethod == null && getterTemplate == null)
            {
                builder.Append(" => ");
                this.WritePropertyCall(builder, references.First(), property, scope, SemanticFacts.IsNullable(this.Compilation, property.Type), true);
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
                    if (references.Count() == 1 && getterTemplate == null && setterTemplate == null)
                    {
                        IMemberInfo reference = references.First();

                        if (property.GetMethod is not null)
                        {
                            builder.AppendIndentation();
                            builder.Append("get => ");
                            this.WritePropertyCall(builder, reference, property, scope, SemanticFacts.IsNullable(this.Compilation, property.Type), true);
                            builder.AppendLine(';');
                        }
                        if (property.SetMethod is not null)
                        {
                            builder.AppendIndentation();
                            builder.Append("set => ");
                            this.WritePropertyCall(builder, reference, property, scope, false, false);
                            builder.AppendLine(" = value;");
                        }
                    }
                    else
                    {
                        if (property.GetMethod is not null)
                        {
                            builder.AppendIndentation();
                            if (getterTemplate != null)
                            {
                                builder.AppendLine("get");
                                builder.AppendIndentation();
                                builder.AppendLine('{');
                                builder.IncrementIndentation();
                                try
                                {
                                    TemplatedSourceTextGenerator generator = new(getterTemplate.Template);

                                    IPropertyModel model = property.IsIndexer ? new PartialIndexerModel() : (IPropertyModel)new PartialPropertyModel();
                                    if (model is IRootModel rootModel)
                                    {
                                        rootModel.Load(this, builder, @interface, scope, references);
                                    }
                                    model.Load(this, builder, property, scope, references);

                                    bool separatorRequired = false;
                                    generator.Emit(this, builder, model, ref separatorRequired);
                                    if (separatorRequired)
                                    {
                                        builder.AppendLine();
                                    }
                                }
                                finally
                                {
                                    builder.DecrementIndentation();
                                }
                                builder.AppendIndentation();
                                builder.AppendLine('}');
                            }
                            else
                            {
                                builder.Append("get => ");
                                this.WritePropertyCall(builder, references.Last(), property, scope, SemanticFacts.IsNullable(this.Compilation, property.Type), true);
                                builder.AppendLine(';');
                            }
                        }
                        if (property.SetMethod is not null)
                        {
                            builder.AppendIndentation();
                            builder.AppendLine("set");
                            builder.AppendIndentation();
                            builder.AppendLine('{');
                            builder.IncrementIndentation();
                            try
                            {
                                if (setterTemplate != null)
                                {
                                    TemplatedSourceTextGenerator generator = new(setterTemplate.Template);
                                    IPropertyModel model = property.IsIndexer ? new PartialIndexerModel() : (IPropertyModel)new PartialPropertyModel();
                                    if (model is IRootModel rootModel)
                                    {
                                        rootModel.Load(this, builder, @interface, scope, references);
                                    }
                                    model.Load(this, builder, property, scope, references);

                                    bool separatorRequired = false;
                                    generator.Emit(this, builder, model, ref separatorRequired);
                                    if (separatorRequired)
                                    {
                                        builder.AppendLine();
                                    }
                                }
                                else
                                {
                                    foreach (IMemberInfo reference in references)
                                    {
                                        builder.AppendIndentation();
                                        this.WritePropertyCall(builder, reference, property, scope, false, false);
                                        builder.AppendLine(" = value;");
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
            PartialTemplate? adderTemplate = this.GetMatchedTemplates(references, AutoInterfaceTargets.EventAdder, @event.Name);
            PartialTemplate? removerTemplate = this.GetMatchedTemplates(references, AutoInterfaceTargets.EventRemover, @event.Name);

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
                if (references.Count() == 1 && adderTemplate == null && removerTemplate == null)
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
                    if (@event.AddMethod != null)
                    {
                        builder.AppendIndentation();
                        builder.AppendLine("add");
                        builder.AppendIndentation();
                        builder.AppendLine('{');
                        builder.IncrementIndentation();
                        try
                        {
                            if (adderTemplate != null)
                            {
                                TemplatedSourceTextGenerator generator = new(adderTemplate.Template);
                                PartialEventModel model = new();
                                model.Load(this, builder, @interface, scope, references);
                                model.Load(this, builder, @event, scope, references);

                                bool separatorRequired = false;
                                generator.Emit(this, builder, model, ref separatorRequired);
                                if (separatorRequired)
                                {
                                    builder.AppendLine();
                                }
                            }
                            else
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
                        }
                        finally
                        {
                            builder.DecrementIndentation();
                        }
                        builder.AppendIndentation();
                        builder.AppendLine('}');
                    }
                    if (@event.RemoveMethod != null)
                    {
                        builder.AppendIndentation();
                        builder.AppendLine("remove");
                        builder.AppendIndentation();
                        builder.AppendLine('{');
                        builder.IncrementIndentation();
                        try
                        {
                            if (removerTemplate != null)
                            {
                                TemplatedSourceTextGenerator generator = new(removerTemplate.Template);
                                PartialEventModel model = new();
                                model.Load(this, builder, @interface, scope, references);
                                model.Load(this, builder, @event, scope, references);

                                bool separatorRequired = false;
                                generator.Emit(this, builder, model, ref separatorRequired);
                                if (separatorRequired)
                                {
                                    builder.AppendLine();
                                }
                            }
                            else
                            {
                                foreach (IMemberInfo reference in references)
                                {
                                    builder.AppendIndentation();
                                    this.WriteMemberReference(builder, reference, scope, false, false);
                                    builder.Append('.');
                                    this.WriteIdentifier(builder, @event);
                                    builder.AppendLine(" -= value;");
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

        public void WriteNamespaceBeginning(SourceBuilder builder, INamespaceSymbol @namespace)
        {
            if (@namespace != null && @namespace.ConstituentNamespaces.Length > 0)
            {
                builder.AppendIndentation();
                builder.Append("namespace");
                builder.Append(' ');
                builder.AppendLine(GetSourceIdentifier(@namespace));
                builder.AppendIndentation();
                builder.AppendLine('{');
                builder.IncrementIndentation();
            }
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
                    builder.Append(typeIsNullable ? '?' : '!');
                }
                else
                {
                    if (typeIsNullable)
                    {
                        builder.Append('?');
                    }
                }
            }
            else
            {
                if (expressionIsNullable)
                {
                    if (typeIsNullable)
                    {
                        //? builder.Append('E');
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
                        //builder.Append('G');
                    }
                    else
                    {
                        //empty or !
                    }
                }
            }
        }

        public void WritePropertyCall(SourceBuilder builder, IMemberInfo reference, IPropertySymbol property, ScopeInfo scope, bool typeIsNullable, bool allowCoalescing)
        {
            this.WriteMemberReference(builder, reference, scope, typeIsNullable, allowCoalescing);
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

        private PartialTemplate? GetMatchedTemplates(IEnumerable<IMemberInfo> references, AutoInterfaceTargets target, string name)
        {
            IEnumerable<(PartialTemplate template, ISymbol reference)> matched = references.SelectMany(i => i.TemplateParts.Where(j => j.MemberTargets.HasFlag(target)).Where(j => j.FilterMatch(name)).Select(j => (j, i.Member)));
            return PartialTemplate.PickTemplate(this.Context, matched, name);
        }

        private string GetSourceIdentifier(ISymbol symbol)
        {
            if (symbol is IPropertySymbol propertySymbol && propertySymbol.IsIndexer)
            {
                return "this";
            }
            else if (symbol is INamespaceSymbol ns)
            {
                return ns.Name;
                //return string.Join("+", ns.ConstituentNamespaces.Select(i => i.Name));
                //return $"<{@namespace.Name};{symbol}>" + this.GetSourceIdentifier(@namespace.Name);
            }
            else if (symbol.DeclaringSyntaxReferences.Length == 0)
            {
                return symbol.Name;
            }
            else
            {
                foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
                {
                    SyntaxNode syntax = syntaxReference.GetSyntax();
                    if (syntax is BaseTypeDeclarationSyntax type)
                    {
                        return this.GetSourceIdentifier(type.Identifier);
                    }
                    else if (syntax is MethodDeclarationSyntax method)
                    {
                        return this.GetSourceIdentifier(method.Identifier);
                    }
                    else if (syntax is ParameterSyntax parameter)
                    {
                        return this.GetSourceIdentifier(parameter.Identifier);
                    }
                    else if (syntax is VariableDeclaratorSyntax variableDeclarator)
                    {
                        return this.GetSourceIdentifier(variableDeclarator.Identifier);
                    }
                    else if (syntax is VariableDeclarationSyntax variableDeclaration)
                    {
                        if (variableDeclaration.Variables.Any(i => i.Identifier.IsVerbatimIdentifier()))
                        {
                            return "@" + symbol;
                        }
                        else
                        {
                            return symbol.ToString();
                        }
                    }
                    else if (syntax is BaseFieldDeclarationSyntax field)
                    {
                        if (field.Declaration.Variables.Any(i => i.Identifier.IsVerbatimIdentifier()))
                        {
                            return "@" + symbol;
                        }
                        else
                        {
                            return symbol.ToString();
                        }
                    }
                    else if (syntax is PropertyDeclarationSyntax property)
                    {
                        return this.GetSourceIdentifier(property.Identifier);
                    }
                    else if (syntax is IndexerDeclarationSyntax)
                    {
                        throw new InvalidOperationException("trying to resolve indexer name");
                    }
                    else if (syntax is EventDeclarationSyntax @event)
                    {
                        return this.GetSourceIdentifier(@event.Identifier);
                    }
                    else if (syntax is TypeParameterSyntax typeParameter)
                    {
                        return this.GetSourceIdentifier(typeParameter.Identifier);
                    }
                    else if (syntax is TupleTypeSyntax)
                    {
                        return symbol.Name;
                    }
                    else if (syntax is TupleElementSyntax tupleElement)
                    {
                        return this.GetSourceIdentifier(tupleElement.Identifier);
                    }
                    else if (syntax is NamespaceDeclarationSyntax @namespace)
                    {
                        throw new NotSupportedException(syntax.GetType().ToString());
                    }
                    else
                    {
                        throw new NotSupportedException(syntax.GetType().ToString());
                    }
                }

                throw new NotSupportedException();
            }
        }

        private string GetSourceIdentifier(SyntaxToken identifier)
        {
            if (identifier.IsVerbatimIdentifier())
            {
                return "@" + identifier.ValueText;
            }
            else
            {
                return identifier.ValueText;
            }
        }

        private string GetSourceIdentifier(NameSyntax name)
        {
            if (name is SimpleNameSyntax simpleName)
            {
                return this.GetSourceIdentifier(simpleName.Identifier);
            }
            else if (name is QualifiedNameSyntax qualifiedName)
            {
                string left = this.GetSourceIdentifier(qualifiedName.Left);
                string right = this.GetSourceIdentifier(qualifiedName.Right);
                if (string.IsNullOrEmpty(left))
                {
                    if (string.IsNullOrEmpty(right))
                    {
                        throw new NotSupportedException("both are null_or_empty.");
                    }
                    else
                    {
                        return right;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(right))
                    {
                        return left;
                    }
                    else
                    {
                        return left + "." + right;
                    }
                }
            }
            else
            {
                throw new NotSupportedException(name.GetType().ToString());
            }
        }

        #endregion
    }
}