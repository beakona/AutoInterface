#define PEEK_0

using BeaKona.AutoInterfaceGenerator.Templates;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BeaKona.AutoInterfaceGenerator;

[Generator]
public sealed class AutoInterfaceSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;
        if (compilation is CSharpCompilation)
        {
            //retrieve the populated receiver
            if (context.SyntaxReceiver is SyntaxReceiver receiver)
            {
                // get newly bound attribute
                if (compilation.GetTypeByMetadataName(typeof(AutoInterfaceAttribute).FullName) is INamedTypeSymbol autoInterfaceAttributeSymbol && compilation.GetTypeByMetadataName(typeof(AutoInterfaceTemplateAttribute).FullName) is INamedTypeSymbol autoInterfaceTemplateAttributeSymbol)
                {
                    // loop over the candidates, and keep the ones that are actually annotated
                    List<AutoInterfaceRecord> records = [];

                    HashSet<IFieldSymbol> fields = [];
                    HashSet<IPropertySymbol> properties = [];

                    foreach (var fieldSyntax in receiver.Fields)
                    {
                        SemanticModel model = compilation.GetSemanticModel(fieldSyntax.SyntaxTree);

                        foreach (VariableDeclaratorSyntax variableSyntax in fieldSyntax.Declaration.Variables)
                        {
                            if (model.GetDeclaredSymbol(variableSyntax) is IFieldSymbol field)
                            {
                                fields.Add(field);
                            }
                        }
                    }

                    foreach (var propertySyntax in receiver.Properties)
                    {
                        SemanticModel model = compilation.GetSemanticModel(propertySyntax.SyntaxTree);

                        if (model.GetDeclaredSymbol(propertySyntax) is IPropertySymbol property)
                        {
                            properties.Add(property);
                        }
                    }

                    foreach (var parameterSyntax in receiver.Parameters)
                    {
                        SemanticModel model = compilation.GetSemanticModel(parameterSyntax.SyntaxTree);
                        if (model.GetDeclaredSymbol(parameterSyntax) is IParameterSymbol parameter)
                        {
                            if (parameter.ContainingType.GetMembers(parameter.Name).FirstOrDefault() is IPropertySymbol property)
                            {
                                properties.Add(property);
                            }
                        }
                    }

                    foreach (var field in fields)
                    {
                        records.AddRange(GetRecords(context, field, field.Type, autoInterfaceAttributeSymbol, autoInterfaceTemplateAttributeSymbol));
                    }

                    foreach (var property in properties)
                    {
                        if (property.IsWriteOnly)
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG06, property, property.Name);
                            continue;
                        }

                        records.AddRange(GetRecords(context, property, property.Type, autoInterfaceAttributeSymbol, autoInterfaceTemplateAttributeSymbol));
                    }

                    var missingAttributes = new TypeRegistry();

                    // group elements by the containing class, and generate the source
                    foreach (IGrouping<INamedTypeSymbol, AutoInterfaceRecord> recordsByContainingType in records.GroupBy<AutoInterfaceRecord, INamedTypeSymbol>(i => i.Member.ContainingType, SymbolEqualityComparer.Default))
                    {
                        INamedTypeSymbol containingType = recordsByContainingType.Key;

                        if (containingType.IsPartial() == false)
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG01, containingType, containingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }

                        if (containingType.IsStatic)
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG02, containingType, containingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }

                        if (containingType.TypeKind != TypeKind.Class && containingType.TypeKind != TypeKind.Struct)
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG08, recordsByContainingType.First().Member);
                            continue;
                        }

                        try
                        {
                            string? code = AutoInterfaceSourceGenerator.GetSourceCodeForType(context, compilation, containingType, recordsByContainingType, missingAttributes);
                            if (code != null)
                            {
                                string name = containingType.Arity > 0 ? $"{containingType.Name}_{containingType.Arity}" : containingType.Name;
#if PEEK_1
                                GeneratePreview(context, name, code);
#else
                                context.AddSource($"{name}_AutoInterface.g.cs", SourceText.From(code, Encoding.UTF8));
#endif
                            }
                        }
                        catch (Exception ex)
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG09, containingType, ex.ToString().Replace("\r", "").Replace("\n", ""));
                        }
                    }

                    if (missingAttributes.Count() > 0)
                    {
                        var toEmit = missingAttributes.Where(i => compilation.IsVisible(i) == false).ToList();
                        if (toEmit.Count > 0)
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG17, (Location?)null, string.Join(", ", toEmit.Select(i => i.ToDisplayString())));
                        }
                    }
                }
            }
        }
    }

#if PEEK_1
    private static void GeneratePreview(GeneratorExecutionContext context, string name, string code)
    {
        var output = new StringBuilder();
        output.AppendLine("namespace BeaKona.Output {");
        output.AppendLine($"public static class Debug_{name}");
        output.AppendLine("{");
        output.AppendLine($"public static readonly string Info = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(code ?? ""))}\"));");
        output.AppendLine("}");
        output.AppendLine("}");
        context.AddSource($"Output_Debug_{name}.g.cs", SourceText.From(output.ToString(), Encoding.UTF8));
    }
#endif

    private static List<PartialTemplate> GetTemplateRecords(GeneratorExecutionContext context, ISymbol directSymbol, ITypeSymbol receiverType, INamedTypeSymbol autoInterfaceTemplateAttributeSymbol, Action<ISymbol, INamedTypeSymbol> registerInterface)
    {
        List<PartialTemplate> templateParts = [];

        foreach (AttributeData attribute in directSymbol.GetAttributes())
        {
            if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(autoInterfaceTemplateAttributeSymbol, SymbolEqualityComparer.Default))
            {
                AutoInterfaceTargets memberTargets = (AutoInterfaceTargets)Convert.ToInt32(attribute.ConstructorArguments[0].Value);

                ITypeSymbol? type = receiverType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

                if (type.TypeKind == TypeKind.Interface)
                {
                    if (type is INamedTypeSymbol interfaceType)
                    {
                        if (IsImplementedDirectly(receiverType, interfaceType))
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
                                    Diagnostics.Report(context, Diagnostics.BKAG12, attribute.ApplicationSyntaxReference?.GetSyntax());
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
                                        Diagnostics.Report(context, Diagnostics.BKAG15, attribute.ApplicationSyntaxReference?.GetSyntax(), memberFilter);
                                        continue;
                                    }
                                }

                                templateParts.Add(new PartialTemplate(memberTargets, rxMemberFilter, new TemplateDefinition(templateLanguage ?? "scriban", templateBody.Trim())));
                                registerInterface(directSymbol, interfaceType);
                            }
                            else if (templateFileName != null && templateFileName.Trim().Length > 0)
                            {
                                string? content = null;

                                AdditionalText? file = context.AdditionalFiles.FirstOrDefault(i => i.Path.EndsWith(templateFileName));
                                if (file != null)
                                {
                                    content = file.GetText()?.ToString()?.Trim() ?? "";
                                }
                                else
                                {
                                    Diagnostics.Report(context, Diagnostics.BKAG11, attribute.ApplicationSyntaxReference?.GetSyntax(), templateFileName);
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
                                        Diagnostics.Report(context, Diagnostics.BKAG15, attribute.ApplicationSyntaxReference?.GetSyntax(), memberFilter);
                                        continue;
                                    }
                                }

                                templateParts.Add(new PartialTemplate(memberTargets, rxMemberFilter, new TemplateDefinition(templateLanguage ?? "scriban", content)));
                                registerInterface(directSymbol, interfaceType);
                            }
                            else
                            {
                                Diagnostics.Report(context, Diagnostics.BKAG14, attribute.ApplicationSyntaxReference?.GetSyntax());
                                continue;
                            }
                        }
                        else
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG04, attribute.ApplicationSyntaxReference?.GetSyntax(), receiverType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }
                    }
                    else
                    {
                        Diagnostics.Report(context, Diagnostics.BKAG09, attribute.ApplicationSyntaxReference?.GetSyntax(), 157834);
                        continue;
                    }
                }
                else
                {
                    Diagnostics.Report(context, Diagnostics.BKAG03, attribute.ApplicationSyntaxReference?.GetSyntax(), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                    continue;
                }
            }
        }

        return templateParts;
    }

    private static List<AutoInterfaceRecord> GetEmbeddedRecords(GeneratorExecutionContext context, ISymbol directSymbol, ITypeSymbol receiverType, INamedTypeSymbol autoInterfaceAttributeSymbol, IEnumerable<PartialTemplate> templateParts, Action<ISymbol> unregisterInterface)
    {
        List<AutoInterfaceRecord> records = [];

        foreach (AttributeData attribute in directSymbol.GetAttributes())
        {
            if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(autoInterfaceAttributeSymbol, SymbolEqualityComparer.Default))
            {
                ITypeSymbol? type = null;
                bool? includeBaseInterfaces = null;
                bool? preferCoalesce = null;
                bool? allowMissingMembers = null;
                MemberMatchTypes? memberMatch = null;

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
                    Diagnostics.Report(context, Diagnostics.BKAG07, attribute.ApplicationSyntaxReference?.GetSyntax());
                    continue;
                }
                else if (type.TypeKind == TypeKind.Interface)
                {
                    if (type is INamedTypeSymbol interfaceType)
                    {
                        string? templateBody = null;
                        string? templateFileName = null;
                        string? templateLanguage = null;

                        #region collect named arguments

                        foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                        {
                            switch (arg.Key)
                            {
                                case nameof(AutoInterfaceAttribute.TemplateFileName):
                                    {
                                        if (arg.Value.Value is string s)
                                        {
                                            templateFileName = s;
                                        }
                                    }
                                    break;
                                case nameof(AutoInterfaceAttribute.TemplateBody):
                                    {
                                        if (arg.Value.Value is string s)
                                        {
                                            templateBody = s;
                                        }
                                    }
                                    break;
                                case nameof(AutoInterfaceAttribute.TemplateLanguage):
                                    {
                                        if (arg.Value.Value is string s)
                                        {
                                            templateLanguage = s;
                                        }
                                    }
                                    break;
                                case nameof(AutoInterfaceAttribute.IncludeBaseInterfaces):
                                    {
                                        if (arg.Value.Value is bool b)
                                        {
                                            includeBaseInterfaces = b;
                                        }
                                    }
                                    break;
                                case nameof(AutoInterfaceAttribute.PreferCoalesce):
                                    {
                                        if (arg.Value.Value is bool b)
                                        {
                                            preferCoalesce = b;
                                        }
                                    }
                                    break;
                                case nameof(AutoInterfaceAttribute.AllowMissingMembers):
                                    {
                                        if (arg.Value.Value is bool b)
                                        {
                                            allowMissingMembers = b;
                                        }
                                    }
                                    break;
                                case nameof(AutoInterfaceAttribute.MemberMatch):
                                    {
                                        if (arg.Value.Value is object value)
                                        {
                                            memberMatch = (MemberMatchTypes)Convert.ChangeType(value, typeof(MemberMatchTypes).GetEnumUnderlyingType());
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
                                Diagnostics.Report(context, Diagnostics.BKAG12, attribute.ApplicationSyntaxReference?.GetSyntax());
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
                                content = file.GetText()?.ToString()?.Trim() ?? "";
                            }
                            else
                            {
                                Diagnostics.Report(context, Diagnostics.BKAG11, attribute.ApplicationSyntaxReference?.GetSyntax(), templateFileName);
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

                            if (templateParts.Any())
                            {
                                Diagnostics.Report(context, Diagnostics.BKAG13, attribute.ApplicationSyntaxReference?.GetSyntax());
                                continue;
                            }

                            template = new TemplateDefinition(templateLanguage ?? "scriban", content);
                        }

                        includeBaseInterfaces ??= false;
                        preferCoalesce ??= true;
                        allowMissingMembers ??= false;
                        memberMatch ??= MemberMatchTypes.Explicit;

                        if (IsImplementedDirectly(receiverType, type))
                        {
                            unregisterInterface(directSymbol);
                            records.Add(new AutoInterfaceRecord(directSymbol, receiverType, interfaceType, template, templateParts, false, preferCoalesce.Value, allowMissingMembers.Value, memberMatch.Value));
                            if (includeBaseInterfaces.Value)
                            {
                                foreach (INamedTypeSymbol baseInterfaceType in interfaceType.AllInterfaces)
                                {
                                    records.Add(new AutoInterfaceRecord(directSymbol, receiverType, baseInterfaceType, template, templateParts, false, preferCoalesce.Value, allowMissingMembers.Value, memberMatch.Value));
                                }
                            }
                        }
                        else if (IsDuckImplementation(receiverType, type, includeBaseInterfaces.Value, allowMissingMembers.Value))
                        {
                            unregisterInterface(directSymbol);
                            records.Add(new AutoInterfaceRecord(directSymbol, receiverType, interfaceType, template, templateParts, true, preferCoalesce.Value, allowMissingMembers.Value, memberMatch.Value));
                            if (includeBaseInterfaces.Value)
                            {
                                foreach (INamedTypeSymbol baseInterfaceType in interfaceType.AllInterfaces)
                                {
                                    bool byType = receiverType.IsMatchByTypeOrImplementsInterface(baseInterfaceType);
                                    records.Add(new AutoInterfaceRecord(directSymbol, receiverType, baseInterfaceType, template, templateParts, !byType, preferCoalesce.Value, allowMissingMembers.Value, memberMatch.Value));
                                }
                            }
                        }
                        else
                        {
                            Diagnostics.Report(context, Diagnostics.BKAG04, attribute.ApplicationSyntaxReference?.GetSyntax(), receiverType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                            continue;
                        }
                    }
                    else
                    {
                        Diagnostics.Report(context, Diagnostics.BKAG09, attribute.ApplicationSyntaxReference?.GetSyntax(), 157834);
                        continue;
                    }
                }
                else
                {
                    Diagnostics.Report(context, Diagnostics.BKAG03, attribute.ApplicationSyntaxReference?.GetSyntax(), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                    continue;
                }
            }
        }

        return records;
    }

    private static List<AutoInterfaceRecord> GetRecords(GeneratorExecutionContext context, ISymbol directSymbol, ITypeSymbol receiverType, INamedTypeSymbol autoInterfaceAttributeSymbol, INamedTypeSymbol autoInterfaceTemplateAttributeSymbol)
    {
        var danglingInterfaceTypesBySymbols = new Dictionary<ISymbol, HashSet<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
        void RegisterPossibleDanglingInterface(ISymbol symbol, INamedTypeSymbol interfaceType)
        {
            if (danglingInterfaceTypesBySymbols.TryGetValue(symbol, out HashSet<INamedTypeSymbol> interfaceTypes) == false)
            {
                danglingInterfaceTypesBySymbols[symbol] = interfaceTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            }

            interfaceTypes.Add(interfaceType);
        }

        void UnregisterDanglingInterface(ISymbol symbol)
        {
            danglingInterfaceTypesBySymbols.Remove(symbol);
        }

        var templateParts = GetTemplateRecords(context, directSymbol, receiverType, autoInterfaceTemplateAttributeSymbol, RegisterPossibleDanglingInterface);

        var records = GetEmbeddedRecords(context, directSymbol, receiverType, autoInterfaceAttributeSymbol, templateParts, UnregisterDanglingInterface).ToList();

        foreach (KeyValuePair<ISymbol, HashSet<INamedTypeSymbol>> danglingInterfaceTypes in danglingInterfaceTypesBySymbols)
        {
            foreach (INamedTypeSymbol interfaceType in danglingInterfaceTypes.Value)
            {
                records.Add(new AutoInterfaceRecord(danglingInterfaceTypes.Key, receiverType, interfaceType, null, templateParts, false, false, false, MemberMatchTypes.Explicit));
            }
        }

        return records;
    }

    private static bool IsImplementedDirectly(ITypeSymbol receiverType, ITypeSymbol interfaceType)
    {
        return receiverType.IsMatchByTypeOrImplementsInterface(interfaceType);
    }

    private static bool IsDuckImplementation(ITypeSymbol receiverType, ITypeSymbol interfaceType, bool includeBaseInterfaces, bool allowMissingMembers)
    {
        if (allowMissingMembers)
        {
            return true;
        }
        else
        {
            return receiverType.IsAllInterfaceMembersImplementedBySignature(interfaceType, true) &&
                (includeBaseInterfaces == false || interfaceType.AllInterfaces.All(i => receiverType.IsMatchByTypeOrImplementsInterface(i) || receiverType.IsAllInterfaceMembersImplementedBySignature(i, true)));
        }
    }

    private static string? GetSourceCodeForType(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol type, IEnumerable<IMemberInfo> members, TypeRegistry missingAttributes)
    {
        var attributeRegistry = new TypeRegistry();

        var scope = new ScopeInfo(type);

        var options = SourceBuilderOptions.Load(context, null);
        var builder = new SourceBuilder([], attributeRegistry, options);

        ICodeTextWriter writer = new CSharpCodeTextWriter(context, compilation);
        bool anyReasonToEmitSourceFile = false;
        bool error = false;

        builder.AppendLine("// <auto-generated />");
        //bool isNullable = compilation.Options.NullableContextOptions == NullableContextOptions.Enable;
        builder.AppendLine("#nullable enable");
        builder.MarkPointForAliases();
        builder.AppendLine();
        bool namespaceGenerated = writer.WriteNamespaceBeginning(builder, type.ContainingNamespace);

        INamedTypeSymbol[] containingTypes = type.GetContainingTypes();

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

        {
            bool first = true;
            foreach (INamedTypeSymbol missingInterfaceType in members.Select(i => i.InterfaceType).Where(i => type.AllInterfaces.Contains(i, SymbolEqualityComparer.Default) == false).ToHashSet())
            {
                builder.Append(first ? " : " : ", ");
                first = false;
                writer.WriteTypeReference(builder, missingInterfaceType, scope);
                anyReasonToEmitSourceFile = true;
            }
        }
        builder.AppendLine();

        builder.AppendIndentation();
        builder.AppendLine('{');
        builder.IncrementIndentation();

        bool separatorRequired = false;

        foreach (IGrouping<INamedTypeSymbol, IMemberInfo> group in members.GroupBy<IMemberInfo, INamedTypeSymbol>(i => i.InterfaceType, SymbolEqualityComparer.Default))
        {
            ISourceTextGenerator? generator = null;
            foreach (IMemberInfo reference in group)
            {
                if (reference.Template is TemplateDefinition td)
                {
                    if (generator is TemplatedSourceTextGenerator g)
                    {
                        if (g.Template.Equals(td) == false)
                        {
                            error = true;
                            Diagnostics.Report(context, Diagnostics.BKAG10, reference.Member);
                            continue;
                        }
                    }
                    else
                    {
                        generator = new TemplatedSourceTextGenerator(td);
                    }
                }
            }

            INamedTypeSymbol @interface = group.Key;
            List<IMemberInfo> references = group.DistinctBy(i => i.Member).ToList();

            bool ShouldGenerate(ISymbol member)
            {
                if (member.IsStatic)
                {
                    return false;
                }

                if (member.DeclaredAccessibility is not Accessibility.Public and Accessibility.Internal)
                {
                    //ignore members with "protected" modifier and similar
                    return false;
                }

                foreach (var reference in references)
                {
                    if (reference.AllowMissingMembers)
                    {
                        if (reference.ReceiverType.IsMemberImplementedBySignature(member, false) == false)
                        {
                            return false;
                        }
                    }
                }

                if (member is IPropertySymbol p && p.ExplicitInterfaceImplementations.Length > 0)
                {
                    return false;
                }

                var memberImplementedBySignature = type.IsMemberImplementedBySignature(member, true);
                var memberImplementedExplicitly = type.IsMemberImplementedExplicitly(member);

                foreach (var reference in references)
                {
                    if (IsMemberImplemented(reference.MemberMatch, memberImplementedBySignature, memberImplementedExplicitly))
                    {
                        return false;
                    }
                }

                return true;
            }

            bool ShouldBeReferenced(ISymbol member, IMemberInfo reference)
            {
                if (reference.AllowMissingMembers)
                {
                    //AllowMissingMembers case
                    return reference.ReceiverType.IsMemberImplementedBySignature(member, false);
                }
                else
                {
                    //strict case
                    return true;
                }
            }

            static bool IsMemberImplemented(MemberMatchTypes memberMatchType, bool memberImplementedBySignature, bool memberImplementedExplicitly)
            {
                return memberMatchType switch
                {
                    MemberMatchTypes.Public => memberImplementedBySignature,
                    MemberMatchTypes.Explicit => memberImplementedExplicitly,
                    MemberMatchTypes.Any => memberImplementedBySignature || memberImplementedExplicitly,
                    _ => throw new NotSupportedException(),
                };
            }

            if (generator != null)
            {
                var model = new StandaloneModel();

                model.Load(writer, builder, @interface, scope, references);

                MethodModel CreateMethod(IMethodSymbol method)
                {
                    var m = new MethodModel();
                    m.Load(writer, builder, method, scope, references.Where(r => ShouldBeReferenced(method, r)).ToList());
                    return m;
                }

                PropertyModel CreateProperty(IPropertySymbol property)
                {
                    var m = new PropertyModel();
                    m.Load(writer, builder, property, scope, references.Where(r => ShouldBeReferenced(property, r)).ToList());
                    return m;
                }

                IndexerModel CreateIndexer(IPropertySymbol indexer)
                {
                    var m = new IndexerModel();
                    m.Load(writer, builder, indexer, scope, references.Where(r => ShouldBeReferenced(indexer, r)).ToList());
                    return m;
                }

                EventModel CreateEvent(IEventSymbol @event)
                {
                    var m = new EventModel();
                    m.Load(writer, builder, @event, scope, references.Where(r => ShouldBeReferenced(@event, r)).ToList());
                    return m;
                }

                model.Methods.AddRange(@interface.GetMethods().Where(ShouldGenerate).Select(CreateMethod));
                model.Properties.AddRange(@interface.GetProperties().Where(ShouldGenerate).Select(CreateProperty));
                model.Indexers.AddRange(@interface.GetIndexers().Where(ShouldGenerate).Select(CreateIndexer));
                model.Events.AddRange(@interface.GetEvents().Where(ShouldGenerate).Select(CreateEvent));

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
                foreach (IMethodSymbol method in @interface.GetMethods().Where(ShouldGenerate))
                {
                    anyReasonToEmitSourceFile = true;

                    if (separatorRequired)
                    {
                        builder.AppendLine();
                    }
                    writer.WriteMethodDefinition(builder, method, scope, @interface, references.Where(r => ShouldBeReferenced(method, r)).ToList());
                    separatorRequired = true;
                }

                foreach (IPropertySymbol property in @interface.GetProperties().Where(ShouldGenerate))
                {
                    anyReasonToEmitSourceFile = true;

                    if (separatorRequired)
                    {
                        builder.AppendLine();
                    }

                    writer.WritePropertyDefinition(builder, property, scope, @interface, references.Where(r => ShouldBeReferenced(property, r)).ToList());
                    separatorRequired = true;
                }

                foreach (IPropertySymbol indexer in @interface.GetIndexers().Where(ShouldGenerate))
                {
                    anyReasonToEmitSourceFile = true;

                    if (separatorRequired)
                    {
                        builder.AppendLine();
                    }

                    writer.WritePropertyDefinition(builder, indexer, scope, @interface, references.Where(r => ShouldBeReferenced(indexer, r)).ToList());
                    separatorRequired = true;
                }

                foreach (IEventSymbol @event in @interface.GetEvents().Where(ShouldGenerate))
                {
                    anyReasonToEmitSourceFile = true;

                    if (separatorRequired)
                    {
                        builder.AppendLine();
                    }

                    writer.WriteEventDefinition(builder, @event, scope, @interface, references.Where(r => ShouldBeReferenced(@event, r)).ToList());
                    separatorRequired = true;
                }
            }
        }

        builder.DecrementIndentation();
        builder.AppendIndentation();
        builder.Append('}');

        for (int i = 0; i < containingTypes.Length; i++)
        {
            builder.AppendLine();
            builder.DecrementIndentation();
            builder.AppendIndentation();
            builder.Append('}');
        }

        if (namespaceGenerated)
        {
            builder.AppendLine();
            builder.DecrementIndentation();
            builder.AppendIndentation();
            builder.Append('}');
        }

        if (builder.Options.InsertFinalNewLine)
        {
            builder.AppendLine();
        }

        if (error == false && anyReasonToEmitSourceFile)
        {
            missingAttributes.AddMany(attributeRegistry);
            return builder.ToString();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    private sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public List<FieldDeclarationSyntax> Fields { get; } = [];
        public List<PropertyDeclarationSyntax> Properties { get; } = [];
        public List<ParameterSyntax> Parameters { get; } = [];

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MemberDeclarationSyntax memberDeclarationSyntax)
            {
                // any field or property with at least one attribute is a candidate for source generation
                if (memberDeclarationSyntax.AttributeLists.Count > 0 && memberDeclarationSyntax.Parent is TypeDeclarationSyntax)
                {
                    if (memberDeclarationSyntax is FieldDeclarationSyntax fieldSyntax)
                    {
                        this.Fields.Add(fieldSyntax);
                    }
                    else if (memberDeclarationSyntax is PropertyDeclarationSyntax propertySyntax)
                    {
                        this.Properties.Add(propertySyntax);
                    }
                }
            }
            else if (syntaxNode is AttributeTargetSpecifierSyntax attributeTargetSyntax)
            {
                //any primary constructor parameter that has attribute with property target
                if (attributeTargetSyntax.Identifier.Text is string attributeTarget)
                {
                    if (string.Equals(attributeTarget, "property", StringComparison.InvariantCulture))
                    {
                        if (ResolveParameter(attributeTargetSyntax) is ParameterSyntax parameterSyntax)
                        {
                            if (ResolveTypeDeclaration(parameterSyntax) is RecordDeclarationSyntax)
                            {
                                this.Parameters.Add(parameterSyntax);
                            }
                        }
                    }
                }

            }
        }

        private static TypeDeclarationSyntax? ResolveTypeDeclaration(ParameterSyntax parameterSyntax)
        {
            return parameterSyntax?.Parent?.Parent as TypeDeclarationSyntax;
        }

        private static ParameterSyntax? ResolveParameter(AttributeTargetSpecifierSyntax attributeTargetSpecifierSyntax)
        {
            return attributeTargetSpecifierSyntax?.Parent?.Parent as ParameterSyntax;
        }
    }
}
