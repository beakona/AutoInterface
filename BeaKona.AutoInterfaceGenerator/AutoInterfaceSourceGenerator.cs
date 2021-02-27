using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace BeaKona.AutoInterfaceGenerator
{
    [Generator]
    public class AutoInterfaceSourceGenerator : ISourceGenerator
    {
        private const string attributeText = @"
#nullable enable
namespace BeaKona
{
    using System;
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class AutoInterfaceAttribute : Attribute
    {
        public AutoInterfaceAttribute()
        {
        }

        public AutoInterfaceAttribute(Type type)
        {
            this.Type = type;
        }

        public Type? Type { get; }
        public string? TemplateLanguage { get; set; }
        public string? TemplateBody { get; set; }
    }
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            SourceText txt = SourceText.From(attributeText, Encoding.UTF8);

            // add the attribute text
            context.AddSource("AutoInterfaceAttribute", txt);

            Compilation compilation = context.Compilation;
            if (compilation is CSharpCompilation c)
            {
                // we're going to create a new compilation that contains the attribute.
                // TODO: we should allow source generators to provide source during initialize, so that this step isn't required.
                if (c.SyntaxTrees.Length > 0 && c.SyntaxTrees[0].Options is CSharpParseOptions options)
                {
                    compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(txt, options));
                }

                //retrieve the populated receiver
                if (context.SyntaxReceiver is SyntaxReceiver receiver)
                {
                    // get the newly bound attribute
                    if (compilation.GetTypeByMetadataName("BeaKona.AutoInterfaceAttribute") is INamedTypeSymbol attributeSymbol)
                    {
                        // loop over the candidates, and keep the ones that are actually annotated
                        List<AutoInterfaceRecord> records = new List<AutoInterfaceRecord>();

                        foreach (MemberDeclarationSyntax candidate in receiver.Candidates)
                        {
                            SemanticModel model = compilation.GetSemanticModel(candidate.SyntaxTree);
                            if (candidate is FieldDeclarationSyntax fieldSyntax)
                            {
                                foreach (VariableDeclaratorSyntax variableSyntax in fieldSyntax.Declaration.Variables)
                                {
                                    // Get the symbol being declared by the member, and keep it if its annotated
                                    if (model.GetDeclaredSymbol(variableSyntax) is IFieldSymbol field)
                                    {
                                        records.AddRange(CollectRecords(context, field, field.Type, attributeSymbol));
                                    }
                                }
                            }
                            else if (candidate is PropertyDeclarationSyntax propertySyntax)
                            {
                                // Get the symbol being declared by the member, and keep it if its annotated
                                if (model.GetDeclaredSymbol(propertySyntax) is IPropertySymbol property)
                                {
                                    if (property.IsWriteOnly)
                                    {
                                        ReportDiagnostic(context, "BK-AG06", nameof(AutoInterfaceResource.AG06_title), nameof(AutoInterfaceResource.AG06_message), nameof(AutoInterfaceResource.AG06_description), DiagnosticSeverity.Error, property,
                                            property.Name);
                                        continue;
                                    }

                                    records.AddRange(CollectRecords(context, property, property.Type, attributeSymbol));
                                }
                            }
                        }

                        // group the elements by class, and generate the source
                        foreach (IGrouping<INamedTypeSymbol, AutoInterfaceRecord> group in records.GroupBy(i => i.Member.ContainingType))
                        {
                            INamedTypeSymbol type = group.Key;

                            bool isPartial = false;
                            foreach (SyntaxReference syntax in type.DeclaringSyntaxReferences)
                            {
                                if (syntax.GetSyntax() is MemberDeclarationSyntax declaration)
                                {
                                    if (declaration.Modifiers.Any(i => i.IsKind(SyntaxKind.PartialKeyword)))
                                    {
                                        isPartial = true;
                                        break;
                                    }
                                }
                            }

                            if (isPartial == false)
                            {
                                ReportDiagnostic(context, "BK-AG01", nameof(AutoInterfaceResource.AG01_title), nameof(AutoInterfaceResource.AG01_message), nameof(AutoInterfaceResource.AG01_description), DiagnosticSeverity.Error, type,
                                    type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                continue;
                            }

                            if (type.IsStatic)
                            {
                                ReportDiagnostic(context, "BK-AG02", nameof(AutoInterfaceResource.AG02_title), nameof(AutoInterfaceResource.AG02_message), nameof(AutoInterfaceResource.AG02_description), DiagnosticSeverity.Error, type,
                                    type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                continue;
                            }

                            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
                            {
                                ReportDiagnostic(context, "BK-AG08", nameof(AutoInterfaceResource.AG08_title), nameof(AutoInterfaceResource.AG08_message), nameof(AutoInterfaceResource.AG08_description), DiagnosticSeverity.Error, group.First().Member);
                                continue;
                            }

                            List<AutoInterfaceRecord> referencesWithMissingInterface = group.Where(i => type.Interfaces.Contains(i.InterfaceType, SymbolEqualityComparer.Default) == false).ToList();

                            if (referencesWithMissingInterface.Count > 0)
                            {
                                HashSet<INamedTypeSymbol> emitted = new HashSet<INamedTypeSymbol>();
                                foreach (AutoInterfaceRecord itemWithMissingInterface in referencesWithMissingInterface)
                                {
                                    if (emitted.Add(itemWithMissingInterface.InterfaceType))
                                    {
                                        ReportDiagnostic(context, "BK-AG05", nameof(AutoInterfaceResource.AG05_title), nameof(AutoInterfaceResource.AG05_message), nameof(AutoInterfaceResource.AG05_description), DiagnosticSeverity.Error, itemWithMissingInterface.Member,
                                            itemWithMissingInterface.InterfaceType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                    }
                                }

                                continue;
                            }

                            try
                            {
                                string? code = AutoInterfaceSourceGenerator.ProcessClass(context, compilation, group.Key, group);
                                if (code != null)
                                {
                                    string name = group.Key.Arity > 0 ? $"{group.Key.Name}_{group.Key.Arity}" : group.Key.Name;
                                    //GeneratePreview(context, name, code);
                                    context.AddSource($"{name}_AutoInterface.cs", SourceText.From(code, Encoding.UTF8));
                                }
                            }
                            catch (Exception ex)
                            {
                                ReportDiagnostic(context, "BK-AG09", nameof(AutoInterfaceResource.AG09_title), nameof(AutoInterfaceResource.AG09_message), nameof(AutoInterfaceResource.AG09_description), DiagnosticSeverity.Error, type,
                                    ex.ToString().Replace("\r", "").Replace("\n", ""));
                            }
                        }
                    }
                }
            }
        }

        //private static void GeneratePreview(GeneratorExecutionContext context, string name, string code)
        //{
        //    StringBuilder output = new StringBuilder();
        //    output.AppendLine("namespace BeaKona.Output {");
        //    output.AppendLine($"public static class Debug_{name}");
        //    output.AppendLine("{");
        //    output.AppendLine($"public static readonly string Info = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(code ?? ""))}\"));");
        //    output.AppendLine("}");
        //    output.AppendLine("}");
        //    context.AddSource($"Output_Debug_{name}.cs", SourceText.From(output.ToString(), Encoding.UTF8));
        //}

        private static List<AutoInterfaceRecord> CollectRecords(GeneratorExecutionContext context, ISymbol symbol, ITypeSymbol receiverType, INamedTypeSymbol attributeSymbol)
        {
            List<AutoInterfaceRecord> records = new List<AutoInterfaceRecord>();

            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                {
                    ITypeSymbol? type = null;
                    if (attribute.ConstructorArguments.Length == 0)
                    {
                        type = receiverType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                    }
                    else
                    {
                        if (attribute.ConstructorArguments[0].Value is ITypeSymbol targetType)
                        {
                            type = targetType;
                        }
                    }

                    if (type == null)
                    {
                        ReportDiagnostic(context, "BK-AG07", nameof(AutoInterfaceResource.AG07_title), nameof(AutoInterfaceResource.AG07_message), nameof(AutoInterfaceResource.AG07_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax());
                        continue;
                    }
                    else if (type.TypeKind == TypeKind.Interface)
                    {
                        if (receiverType.Equals(type, SymbolEqualityComparer.Default) || receiverType.AllInterfaces.Contains(type, SymbolEqualityComparer.Default))
                        {
                            if (type is INamedTypeSymbol interfaceType)
                            {
                                string? templateBody = null;
                                string? templateLanguage = null;

                                #region collect named arguments [only one argument for now]

                                foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                                {
                                    switch (arg.Key)
                                    {
                                        case "TemplateBody":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateBody = s;
                                                }
                                                break;
                                            }
                                        case "TemplateLanguage":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateLanguage = s;
                                                }
                                                break;
                                            }
                                    }
                                }

                                #endregion

                                TemplateSettings? template = null;
                                if (templateBody != null && templateBody.Trim().Length > 0)
                                {
                                    template = new TemplateSettings(templateLanguage ?? "scriban", templateBody.Trim());
                                }

                                records.Add(new AutoInterfaceRecord(symbol, receiverType, attribute, interfaceType, template));
                            }
                            else
                            {
                                ReportDiagnostic(context, "BK-AG09", nameof(AutoInterfaceResource.AG09_title), nameof(AutoInterfaceResource.AG09_message), nameof(AutoInterfaceResource.AG09_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                    157834);
                                continue;
                            }
                        }
                        else
                        {
                            ReportDiagnostic(context, "BK-AG04", nameof(AutoInterfaceResource.AG04_title), nameof(AutoInterfaceResource.AG04_message), nameof(AutoInterfaceResource.AG04_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                receiverType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }
                    }
                    else
                    {
                        ReportDiagnostic(context, "BK-AG03", nameof(AutoInterfaceResource.AG03_title), nameof(AutoInterfaceResource.AG03_message), nameof(AutoInterfaceResource.AG03_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                            type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                        continue;
                    }
                }
            }

            return records;
        }

        private static string? ProcessClass(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol type, IEnumerable<IMemberInfo> infos)
        {
            ScopeInfo scope = new ScopeInfo(type);

            SourceBuilder builder = new SourceBuilder();

            ICodeTextWriter writer = new CSharpCodeTextWriter(compilation);
            bool anyReasonToEmitSourceFile = false;
            bool error = false;

            //bool isNullable = compilation.Options.NullableContextOptions == NullableContextOptions.Enable;
            builder.AppendLine("#nullable enable");

            ImmutableArray<INamespaceSymbol> namespaceParts = type.ContainingNamespace != null && type.ContainingNamespace.IsGlobalNamespace == false ? SemanticFacts.GetNamespaceParts(type.ContainingNamespace) : new ImmutableArray<INamespaceSymbol>();
            if (namespaceParts.Length > 0)
            {
                writer.WriteNamespaceBeginning(builder, namespaceParts);
            }

            List<INamedTypeSymbol> containingTypes = new List<INamedTypeSymbol>();
            for (INamedTypeSymbol? ct = type.ContainingType; ct != null; ct = ct.ContainingType)
            {
                containingTypes.Insert(0, ct);
            }

            foreach (INamedTypeSymbol ct in containingTypes)
            {
                builder.AppendIndentation();
                writer.WriteTypeDeclarationBeginning(builder, ct, new ScopeInfo(ct));
                builder.AppendLine();
                builder.AppendIndentation();
                builder.AppendLine('{');
                builder.IncrementIndentation();
            }

            builder.AppendIndentation();
            writer.WriteTypeDeclarationBeginning(builder, type, scope);
            builder.AppendLine();
            builder.AppendIndentation();
            builder.AppendLine('{');
            builder.IncrementIndentation();

            bool separatorRequired = false;

            foreach (IGrouping<INamedTypeSymbol, IMemberInfo> group in infos.GroupBy(i => i.InterfaceType))
            {
                List<IMemberInfo> references = group.ToList();

                ISourceTextGenerator? generator = null;
                foreach (IMemberInfo reference in references)
                {
                    if (reference.Template is TemplateSettings ts)
                    {
                        if (generator is TemplatedSourceTextGenerator g)
                        {
                            if (g.Settings.Equals(ts) == false)
                            {
                                error = true;
                                ReportDiagnostic(context, "BK-AG10", nameof(AutoInterfaceResource.AG10_title), nameof(AutoInterfaceResource.AG10_message), nameof(AutoInterfaceResource.AG10_description), DiagnosticSeverity.Error, reference.Member);
                                continue;
                            }
                        }
                        else
                        {
                            generator = new TemplatedSourceTextGenerator(ts);
                        }
                    }
                }

                if (generator != null)
                {
                    Model model = new Model(writer, builder, group.Key, type, references, scope);
                    generator.Emit(writer, builder, model, ref separatorRequired, out bool anyReasonToEmitSourceFileLocal);
                    if (anyReasonToEmitSourceFileLocal)
                    {
                        anyReasonToEmitSourceFile = true;
                    }

                    if (separatorRequired)
                    {
                        builder.AppendLine();
                        separatorRequired = false;
                    }
                }
                else
                {
                    foreach (IMethodSymbol method in group.Key.GetMethods().Where(i => type.IsMemberImplemented(i) == false))
                    {
                        anyReasonToEmitSourceFile = true;

                        if (separatorRequired)
                        {
                            builder.AppendLine();
                        }
                        writer.WriteMethodDefinition(builder, method, scope, group.Key, references);
                        separatorRequired = true;
                    }

                    foreach (IPropertySymbol property in group.Key.GetProperties().Where(i => type.IsMemberImplemented(i) == false))
                    {
                        anyReasonToEmitSourceFile = true;

                        if (separatorRequired)
                        {
                            builder.AppendLine();
                        }

                        writer.WritePropertyDefinition(builder, property, scope, group.Key, references);
                        separatorRequired = true;
                    }

                    foreach (IPropertySymbol indexer in group.Key.GetIndexers().Where(i => type.IsMemberImplemented(i) == false))
                    {
                        anyReasonToEmitSourceFile = true;

                        if (separatorRequired)
                        {
                            builder.AppendLine();
                        }

                        writer.WritePropertyDefinition(builder, indexer, scope, group.Key, references);
                        separatorRequired = true;
                    }

                    foreach (IEventSymbol @event in group.Key.GetEvents().Where(i => type.IsMemberImplemented(i) == false))
                    {
                        anyReasonToEmitSourceFile = true;

                        if (separatorRequired)
                        {
                            builder.AppendLine();
                        }

                        writer.WriteEventDefinition(builder, @event, scope, group.Key, references);
                        separatorRequired = true;
                    }
                }
            }

            builder.DecrementIndentation();
            builder.AppendIndentation();
            builder.AppendLine('}');

            for (int i = 0; i < containingTypes.Count; i++)
            {
                builder.DecrementIndentation();
                builder.AppendIndentation();
                builder.AppendLine('}');
            }

            if (namespaceParts.Length > 0)
            {
                builder.DecrementIndentation();
                builder.AppendIndentation();
                builder.AppendLine('}');
            }

            //return error.ToString() + " " + anyReasonToEmitSourceFile.ToString();
            return error == false && anyReasonToEmitSourceFile ? builder.ToString() : null;
        }

        private static void ReportDiagnostic(GeneratorExecutionContext context, string id, string title, string message, string description, DiagnosticSeverity severity, SyntaxNode? node, params object?[] messageArgs)
        {
            AutoInterfaceSourceGenerator.ReportDiagnostic(context, id, title, message, description, severity, node?.GetLocation(), messageArgs);
        }

        private static void ReportDiagnostic(GeneratorExecutionContext context, string id, string title, string message, string description, DiagnosticSeverity severity, ISymbol? member, params object?[] messageArgs)
        {
            AutoInterfaceSourceGenerator.ReportDiagnostic(context, id, title, message, description, severity, member != null && member.Locations.Length > 0 ? member.Locations[0] : null, messageArgs);
        }

        private static void ReportDiagnostic(GeneratorExecutionContext context, string id, string title, string message, string description, DiagnosticSeverity severity, Location? location, params object?[] messageArgs)
        {
            LocalizableString ltitle = new LocalizableResourceString(title, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
            LocalizableString lmessage = new LocalizableResourceString(message, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
            LocalizableString? ldescription = new LocalizableResourceString(description, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
            string category = typeof(AutoInterfaceSourceGenerator).Namespace;
            string? link = "https://github.com/beakona/AutoInterface";
            DiagnosticDescriptor dd = new DiagnosticDescriptor(id, ltitle, lmessage, category, severity, true, ldescription, link, WellKnownDiagnosticTags.NotConfigurable);
            Diagnostic d = Diagnostic.Create(dd, location, messageArgs);
            context.ReportDiagnostic(d);
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MemberDeclarationSyntax> Candidates { get; } = new List<MemberDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is MemberDeclarationSyntax memberDeclarationSyntax && memberDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (memberDeclarationSyntax.Parent is TypeDeclarationSyntax)
                    {
                        // any field or property with at least one attribute is a candidate for source generation
                        if (memberDeclarationSyntax is FieldDeclarationSyntax || memberDeclarationSyntax is PropertyDeclarationSyntax)
                        {
                            this.Candidates.Add(memberDeclarationSyntax);
                        }
                    }
                }
            }
        }
    }
}