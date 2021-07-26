using BeaKona.AutoInterfaceGenerator.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BeaKona.AutoInterfaceGenerator
{
    [Generator]
    public class AutoInterfaceSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            using Stream icStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BeaKona.AutoInterfaceGenerator.InjectedCode.cs");
            using StreamReader icReader = new(icStream);

            SourceText txt = SourceText.From(icReader.ReadToEnd(), Encoding.UTF8);

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
                    if (compilation.GetTypeByMetadataName("BeaKona.AutoInterfaceAttribute") is INamedTypeSymbol autoInterfaceAttributeSymbol && compilation.GetTypeByMetadataName("BeaKona.AutoInterfaceTemplateAttribute") is INamedTypeSymbol autoInterfaceTemplateAttributeSymbol)
                    {
                        // loop over the candidates, and keep the ones that are actually annotated
                        List<AutoInterfaceRecord> records = new();

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
                                        records.AddRange(CollectRecords(context, field, field.Type, autoInterfaceAttributeSymbol, autoInterfaceTemplateAttributeSymbol));
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
                                        Helpers.ReportDiagnostic(context, "BK-AG06", nameof(AutoInterfaceResource.AG06_title), nameof(AutoInterfaceResource.AG06_message), nameof(AutoInterfaceResource.AG06_description), DiagnosticSeverity.Error, property,
                                            property.Name);
                                        continue;
                                    }

                                    records.AddRange(CollectRecords(context, property, property.Type, autoInterfaceAttributeSymbol, autoInterfaceTemplateAttributeSymbol));
                                }
                            }
                        }
                        //inject records of derived interfaces
                        List<AutoInterfaceRecord> derived = records
                            .SelectMany(x => x.ReceiverType.AllInterfaces
                            //.Select(d => new AutoInterfaceRecord(x.Member, d, d, x.Template, x.TemplateParts.ToList())))  <- may be used to prevent explicit cast
                            .Select(d => new AutoInterfaceRecord(x.Member, x.ReceiverType, d, x.Template, x.TemplateParts.ToList())))
                            .ToList();

                        foreach (var d in derived)
                        {
                            if (!records.Contains(d)) records.Add(d);
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
                                Helpers.ReportDiagnostic(context, "BK-AG01", nameof(AutoInterfaceResource.AG01_title), nameof(AutoInterfaceResource.AG01_message), nameof(AutoInterfaceResource.AG01_description), DiagnosticSeverity.Error, type,
                                    type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                continue;
                            }

                            if (type.IsStatic)
                            {
                                Helpers.ReportDiagnostic(context, "BK-AG02", nameof(AutoInterfaceResource.AG02_title), nameof(AutoInterfaceResource.AG02_message), nameof(AutoInterfaceResource.AG02_description), DiagnosticSeverity.Error, type,
                                    type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                continue;
                            }

                            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
                            {
                                Helpers.ReportDiagnostic(context, "BK-AG08", nameof(AutoInterfaceResource.AG08_title), nameof(AutoInterfaceResource.AG08_message), nameof(AutoInterfaceResource.AG08_description), DiagnosticSeverity.Error, group.First().Member);
                                continue;
                            }

                            List<AutoInterfaceRecord> referencesWithMissingInterface = group.Where(i => type.AllInterfaces.Contains(i.InterfaceType, SymbolEqualityComparer.Default) == false).ToList();

                            if (referencesWithMissingInterface.Count > 0)
                            {
                                HashSet<INamedTypeSymbol> emitted = new();
                                foreach (AutoInterfaceRecord itemWithMissingInterface in referencesWithMissingInterface)
                                {
                                    if (emitted.Add(itemWithMissingInterface.InterfaceType))
                                    {
                                        Helpers.ReportDiagnostic(context, "BK-AG05", nameof(AutoInterfaceResource.AG05_title), nameof(AutoInterfaceResource.AG05_message), nameof(AutoInterfaceResource.AG05_description), DiagnosticSeverity.Error, itemWithMissingInterface.Member,
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
                                Helpers.ReportDiagnostic(context, "BK-AG09", nameof(AutoInterfaceResource.AG09_title), nameof(AutoInterfaceResource.AG09_message), nameof(AutoInterfaceResource.AG09_description), DiagnosticSeverity.Error, type,
                                    ex.ToString().Replace("\r", "").Replace("\n", ""));
                            }
                        }
                    }
                }
            }
        }

        //private static void GeneratePreview(GeneratorExecutionContext context, string name, string code)
        //{
        //    StringBuilder output = new();
        //    output.AppendLine("namespace BeaKona.Output {");
        //    output.AppendLine($"public static class Debug_{name}");
        //    output.AppendLine("{");
        //    output.AppendLine($"public static readonly string Info = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(code ?? ""))}\"));");
        //    output.AppendLine("}");
        //    output.AppendLine("}");
        //    context.AddSource($"Output_Debug_{name}.cs", SourceText.From(output.ToString(), Encoding.UTF8));
        //}

        private static List<AutoInterfaceRecord> CollectRecords(GeneratorExecutionContext context, ISymbol symbol, ITypeSymbol receiverType, INamedTypeSymbol autoInterfaceAttributeSymbol, INamedTypeSymbol autoInterfaceTemplateAttributeSymbol)
        {
            List<PartialTemplate> templateParts = new();
            Dictionary<ISymbol, HashSet<INamedTypeSymbol>> danglingInterfaceTypesBySymbols = new();

            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(autoInterfaceTemplateAttributeSymbol, SymbolEqualityComparer.Default))
                {
                    ITypeSymbol? type = receiverType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

                    AutoInterfaceTargets memberTargets = (AutoInterfaceTargets)Convert.ToInt32(attribute.ConstructorArguments[0].Value);

                    if (type.TypeKind == TypeKind.Interface)
                    {
                        if (receiverType.Equals(type, SymbolEqualityComparer.Default) || receiverType.AllInterfaces.Contains(type, SymbolEqualityComparer.Default))
                        {
                            if (type is INamedTypeSymbol interfaceType)
                            {
                                string? templateBody = null;
                                string? templateFileName = null;
                                string? templateLanguage = null;
                                string? memberFilter = null;

                                #region collect named arguments [only one argument for now]

                                foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                                {
                                    switch (arg.Key)
                                    {
                                        case "FileName":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateFileName = s;
                                                }
                                            }
                                            break;
                                        case "Body":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateBody = s;
                                                }
                                            }
                                            break;
                                        case "Language":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateLanguage = s;
                                                }
                                            }
                                            break;
                                        case "Filter":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    memberFilter = s;
                                                }
                                            }
                                            break;
                                    }
                                }

                                #endregion

                                if (templateBody != null && templateBody.Trim().Length > 0)
                                {
                                    if (templateFileName != null && templateFileName.Trim().Length > 0)
                                    {
                                        Helpers.ReportDiagnostic(context, "BK-AG12", nameof(AutoInterfaceResource.AG12_title), nameof(AutoInterfaceResource.AG12_message), nameof(AutoInterfaceResource.AG12_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax()
                                            );
                                        continue;
                                    }

                                    Regex? rxMemberFilter = null;
                                    if (memberFilter != null && memberFilter.Length > 0)
                                    {
                                        try
                                        {
                                            rxMemberFilter = new Regex(memberFilter, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
                                        }
                                        catch
                                        {
                                            Helpers.ReportDiagnostic(context, "BK-AG15", nameof(AutoInterfaceResource.AG15_title), nameof(AutoInterfaceResource.AG15_message), nameof(AutoInterfaceResource.AG15_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                                memberFilter);
                                            continue;
                                        }
                                    }

                                    templateParts.Add(new PartialTemplate(memberTargets, rxMemberFilter, new TemplateDefinition(templateLanguage ?? "scriban", templateBody.Trim())));
                                    if (danglingInterfaceTypesBySymbols.TryGetValue(symbol, out HashSet<INamedTypeSymbol> interfaceTypes) == false)
                                    {
                                        danglingInterfaceTypesBySymbols[symbol] = interfaceTypes = new HashSet<INamedTypeSymbol>();
                                    }

                                    interfaceTypes.Add(interfaceType);
                                }
                                else if (templateFileName != null && templateFileName.Trim().Length > 0)
                                {
                                    string? content = null;

                                    AdditionalText? file = context.AdditionalFiles.FirstOrDefault(i => i.Path.EndsWith(templateFileName));
                                    if (file != null)
                                    {
                                        content = file.GetText()?.ToString()?.Trim();
                                        if (content == null)
                                        {
                                            content = "";
                                        }
                                    }
                                    else
                                    {
                                        Helpers.ReportDiagnostic(context, "BK-AG11", nameof(AutoInterfaceResource.AG11_title), nameof(AutoInterfaceResource.AG11_message), nameof(AutoInterfaceResource.AG11_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                            templateFileName);
                                        continue;
                                    }

                                    if (string.IsNullOrEmpty(templateLanguage))
                                    {
                                        string extension = Path.GetExtension(templateFileName).ToLowerInvariant();
                                        if (extension.StartsWith("."))
                                        {
                                            extension = extension.Substring(1);
                                        }
                                        templateLanguage = extension;
                                    }

                                    Regex? rxMemberFilter = null;
                                    if (memberFilter != null && memberFilter.Length > 0)
                                    {
                                        try
                                        {
                                            rxMemberFilter = new Regex(memberFilter, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
                                        }
                                        catch
                                        {
                                            Helpers.ReportDiagnostic(context, "BK-AG15", nameof(AutoInterfaceResource.AG15_title), nameof(AutoInterfaceResource.AG15_message), nameof(AutoInterfaceResource.AG15_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                                memberFilter);
                                            continue;
                                        }
                                    }

                                    templateParts.Add(new PartialTemplate(memberTargets, rxMemberFilter, new TemplateDefinition(templateLanguage ?? "scriban", content)));
                                    if (danglingInterfaceTypesBySymbols.TryGetValue(symbol, out HashSet<INamedTypeSymbol> interfaceTypes) == false)
                                    {
                                        danglingInterfaceTypesBySymbols[symbol] = interfaceTypes = new HashSet<INamedTypeSymbol>();
                                    }

                                    interfaceTypes.Add(interfaceType);
                                }
                                else
                                {
                                    Helpers.ReportDiagnostic(context, "BK-AG14", nameof(AutoInterfaceResource.AG14_title), nameof(AutoInterfaceResource.AG14_message), nameof(AutoInterfaceResource.AG14_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax()
                                        );
                                    continue;
                                }
                            }
                            else
                            {
                                Helpers.ReportDiagnostic(context, "BK-AG09", nameof(AutoInterfaceResource.AG09_title), nameof(AutoInterfaceResource.AG09_message), nameof(AutoInterfaceResource.AG09_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                    157834);
                                continue;
                            }
                        }
                        else
                        {
                            Helpers.ReportDiagnostic(context, "BK-AG04", nameof(AutoInterfaceResource.AG04_title), nameof(AutoInterfaceResource.AG04_message), nameof(AutoInterfaceResource.AG04_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                receiverType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }
                    }
                    else
                    {
                        Helpers.ReportDiagnostic(context, "BK-AG03", nameof(AutoInterfaceResource.AG03_title), nameof(AutoInterfaceResource.AG03_message), nameof(AutoInterfaceResource.AG03_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                            type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                        continue;
                    }
                }
            }

            List<AutoInterfaceRecord> records = new();

            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(autoInterfaceAttributeSymbol, SymbolEqualityComparer.Default))
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
                        Helpers.ReportDiagnostic(context, "BK-AG07", nameof(AutoInterfaceResource.AG07_title), nameof(AutoInterfaceResource.AG07_message), nameof(AutoInterfaceResource.AG07_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax());
                        continue;
                    }
                    else if (type.TypeKind == TypeKind.Interface)
                    {
                        if (receiverType.Equals(type, SymbolEqualityComparer.Default) || receiverType.AllInterfaces.Contains(type, SymbolEqualityComparer.Default))
                        {
                            if (type is INamedTypeSymbol interfaceType)
                            {
                                string? templateBody = null;
                                string? templateFileName = null;
                                string? templateLanguage = null;

                                #region collect named arguments [only one argument for now]

                                foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                                {
                                    switch (arg.Key)
                                    {
                                        case "TemplateFileName":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateFileName = s;
                                                }
                                            }
                                            break;
                                        case "TemplateBody":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateBody = s;
                                                }
                                            }
                                            break;
                                        case "TemplateLanguage":
                                            {
                                                if (arg.Value.Value is string s)
                                                {
                                                    templateLanguage = s;
                                                }
                                            }
                                            break;
                                    }
                                }

                                #endregion

                                TemplateDefinition? template = null;
                                if (templateBody != null && templateBody.Trim().Length > 0)
                                {
                                    if (templateFileName != null && templateFileName.Trim().Length > 0)
                                    {
                                        Helpers.ReportDiagnostic(context, "BK-AG12", nameof(AutoInterfaceResource.AG12_title), nameof(AutoInterfaceResource.AG12_message), nameof(AutoInterfaceResource.AG12_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax()
                                            );
                                        continue;
                                    }

                                    template = new TemplateDefinition(templateLanguage ?? "scriban", templateBody.Trim());
                                }
                                else if (templateFileName != null && templateFileName.Trim().Length > 0)
                                {
                                    string? content = null;

                                    AdditionalText? file = context.AdditionalFiles.FirstOrDefault(i => i.Path.EndsWith(templateFileName));
                                    if (file != null)
                                    {
                                        content = file.GetText()?.ToString()?.Trim();
                                        if (content == null)
                                        {
                                            content = "";
                                        }
                                    }
                                    else
                                    {
                                        Helpers.ReportDiagnostic(context, "BK-AG11", nameof(AutoInterfaceResource.AG11_title), nameof(AutoInterfaceResource.AG11_message), nameof(AutoInterfaceResource.AG11_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                            templateFileName);
                                        continue;
                                    }

                                    if (string.IsNullOrEmpty(templateLanguage))
                                    {
                                        string extension = Path.GetExtension(templateFileName).ToLowerInvariant();
                                        if (extension.StartsWith("."))
                                        {
                                            extension = extension.Substring(1);
                                        }
                                        templateLanguage = extension;
                                    }

                                    if (templateParts.Count > 0)
                                    {
                                        Helpers.ReportDiagnostic(context, "BK-AG13", nameof(AutoInterfaceResource.AG13_title), nameof(AutoInterfaceResource.AG13_message), nameof(AutoInterfaceResource.AG13_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax()
                                            );
                                        continue;
                                    }

                                    template = new TemplateDefinition(templateLanguage ?? "scriban", content);
                                }

                                records.Add(new AutoInterfaceRecord(symbol, receiverType, interfaceType, template, templateParts));
                                danglingInterfaceTypesBySymbols.Remove(symbol);
                            }
                            else
                            {
                                Helpers.ReportDiagnostic(context, "BK-AG09", nameof(AutoInterfaceResource.AG09_title), nameof(AutoInterfaceResource.AG09_message), nameof(AutoInterfaceResource.AG09_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                    157834);
                                continue;
                            }
                        }
                        else
                        {
                            Helpers.ReportDiagnostic(context, "BK-AG04", nameof(AutoInterfaceResource.AG04_title), nameof(AutoInterfaceResource.AG04_message), nameof(AutoInterfaceResource.AG04_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                receiverType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }
                    }
                    else
                    {
                        Helpers.ReportDiagnostic(context, "BK-AG03", nameof(AutoInterfaceResource.AG03_title), nameof(AutoInterfaceResource.AG03_message), nameof(AutoInterfaceResource.AG03_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                            type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                        continue;
                    }
                }
            }

            foreach (KeyValuePair<ISymbol, HashSet<INamedTypeSymbol>> danglingInterfaceTypes in danglingInterfaceTypesBySymbols)
            {
                foreach (INamedTypeSymbol interfaceType in danglingInterfaceTypes.Value)
                {
                    records.Add(new AutoInterfaceRecord(danglingInterfaceTypes.Key, receiverType, interfaceType, null, templateParts));
                }
            }

            return records;
        }

        private static string? ProcessClass(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol type, IEnumerable<IMemberInfo> infos)
        {
            ScopeInfo scope = new(type);

            SourceBuilder builder = new();

            ICodeTextWriter writer = new CSharpCodeTextWriter(context, compilation);
            bool anyReasonToEmitSourceFile = false;
            bool error = false;

            //bool isNullable = compilation.Options.NullableContextOptions == NullableContextOptions.Enable;
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            writer.WriteNamespaceBeginning(builder, type.ContainingNamespace);

            List<INamedTypeSymbol> containingTypes = new();
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
                List<IMemberInfo> references = group.DistinctBy(x => x.Member.ContainingType).ToList();

                ISourceTextGenerator? generator = null;
                foreach (IMemberInfo reference in references)
                {
                    if (reference.Template is TemplateDefinition td)
                    {
                        if (generator is TemplatedSourceTextGenerator g)
                        {
                            if (g.Template.Equals(td) == false)
                            {
                                error = true;
                                Helpers.ReportDiagnostic(context, "BK-AG10", nameof(AutoInterfaceResource.AG10_title), nameof(AutoInterfaceResource.AG10_message), nameof(AutoInterfaceResource.AG10_description), DiagnosticSeverity.Error, reference.Member);
                                continue;
                            }
                        }
                        else
                        {
                            generator = new TemplatedSourceTextGenerator(td);
                        }
                    }
                }

                if (generator != null)
                {
                    INamedTypeSymbol @interface = group.Key;

                    StandaloneModel model = new();

                    model.Load(writer, builder, @interface, scope, references);

                    MethodModel CreateMethod(IMethodSymbol method)
                    {
                        MethodModel m = new();
                        m.Load(writer, builder, method, scope, references);
                        return m;
                    }


                    PropertyModel CreateProperty(IPropertySymbol property)
                    {
                        PropertyModel m = new();
                        m.Load(writer, builder, property, scope, references);
                        return m;
                    }

                    IndexerModel CreateIndexer(IPropertySymbol indexer)
                    {
                        IndexerModel m = new();
                        m.Load(writer, builder, indexer, scope, references);
                        return m;
                    }

                    EventModel CreateEvent(IEventSymbol @event)
                    {
                        EventModel m = new();
                        m.Load(writer, builder, @event, scope, references);
                        return m;
                    }

                    model.Methods.AddRange(@interface.GetMethods().Where(i => type.IsMemberImplemented(i) == false).Select(CreateMethod));
                    model.Properties.AddRange(@interface.GetProperties().Where(i => type.IsMemberImplemented(i) == false).Select(CreateProperty));
                    model.Indexers.AddRange(@interface.GetIndexers().Where(i => type.IsMemberImplemented(i) == false).Select(CreateIndexer));
                    model.Events.AddRange(@interface.GetEvents().Where(i => type.IsMemberImplemented(i) == false).Select(CreateEvent));

                    generator.Emit(writer, builder, model, ref separatorRequired);
                    anyReasonToEmitSourceFile = true;

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

            if (type.ContainingNamespace != null && type.ContainingNamespace.ConstituentNamespaces.Length > 0)
            {
                builder.DecrementIndentation();
                builder.AppendIndentation();
                builder.AppendLine('}');
            }

            return error == false && anyReasonToEmitSourceFile ? builder.ToString() : null;
        }


        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MemberDeclarationSyntax> Candidates { get; } = new();

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