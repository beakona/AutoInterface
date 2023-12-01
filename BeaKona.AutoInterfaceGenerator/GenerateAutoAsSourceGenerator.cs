#define PEEK_0

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace BeaKona.AutoInterfaceGenerator;

[Generator]
public sealed class GenerateAutoAsSourceGenerator : ISourceGenerator
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
                if (compilation.GetTypeByMetadataName(typeof(GenerateAutoAsAttribute).FullName) is INamedTypeSymbol generateAutoAsAttributeSymbol)
                {
                    GenerateAutoAsAttribute? GetGenerateAutoAsAttribute(INamedTypeSymbol type)
                    {
                        foreach (AttributeData attribute in type.GetAttributes())
                        {
                            if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(generateAutoAsAttributeSymbol, SymbolEqualityComparer.Default))
                            {
                                var result = new GenerateAutoAsAttribute();

                                foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                                {
                                    switch (arg.Key)
                                    {
                                        case nameof(GenerateAutoAsAttribute.EntireInterfaceHierarchy):
                                            {
                                                if (arg.Value.Value is bool b)
                                                {
                                                    result.EntireInterfaceHierarchy = b;
                                                }
                                            }
                                            break;
                                        case nameof(GenerateAutoAsAttribute.SkipSystemInterfaces):
                                            {
                                                if (arg.Value.Value is bool b)
                                                {
                                                    result.SkipSystemInterfaces = b;
                                                }
                                            }
                                            break;
                                    }
                                }

                                return result;
                            }
                        }

                        return null;
                    }

                    var types = new List<INamedTypeSymbol>();

                    foreach (TypeDeclarationSyntax candidate in receiver.Candidates)
                    {
                        SemanticModel model = compilation.GetSemanticModel(candidate.SyntaxTree);

                        if (model.GetDeclaredSymbol(candidate) is INamedTypeSymbol type)
                        {
                            if (GetGenerateAutoAsAttribute(type) is GenerateAutoAsAttribute attribute)
                            {
                                if (type.IsPartial() == false)
                                {
                                    Helpers.ReportDiagnostic(context, "BKAG01", nameof(AutoInterfaceResource.AG01_title), nameof(AutoInterfaceResource.AG01_message), nameof(AutoInterfaceResource.AG01_description), DiagnosticSeverity.Error, type,
                                        type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                    continue;
                                }

                                if (type.IsStatic)
                                {
                                    Helpers.ReportDiagnostic(context, "BKAG02", nameof(AutoInterfaceResource.AG02_title), nameof(AutoInterfaceResource.AG02_message), nameof(AutoInterfaceResource.AG02_description), DiagnosticSeverity.Error, type,
                                        type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                    continue;
                                }

                                if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
                                {
                                    Helpers.ReportDiagnostic(context, "BKAG08", nameof(AutoInterfaceResource.AG08_title), nameof(AutoInterfaceResource.AG08_message), nameof(AutoInterfaceResource.AG08_description), DiagnosticSeverity.Error, type);
                                    continue;
                                }

                                try
                                {
                                    string? code = GenerateAutoAsSourceGenerator.ProcessClass(context, compilation, type, attribute);
                                    if (code != null)
                                    {
                                        string name = type.Arity > 0 ? $"{type.Name}_{type.Arity}" : type.Name;
#if PEEK_1
                                        GeneratePreview(context, name, code);
#else
                                        context.AddSource($"{name}_GenerateAutoAs.g.cs", SourceText.From(code, Encoding.UTF8));
#endif
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Helpers.ReportDiagnostic(context, "BKAG09", nameof(AutoInterfaceResource.AG09_title), nameof(AutoInterfaceResource.AG09_message), nameof(AutoInterfaceResource.AG09_description), DiagnosticSeverity.Error, type,
                                        ex.ToString().Replace("\r", "").Replace("\n", ""));
                                }
                            }
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

    private static string? ProcessClass(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol type, GenerateAutoAsAttribute attribute)
    {
        var scope = new ScopeInfo(type);

        List<INamedTypeSymbol> interfaces;

        if (attribute.EntireInterfaceHierarchy)
        {
            interfaces = type.AllInterfaces.Where(i => i.CanBeReferencedByName).ToList();
        }
        else
        {
            interfaces = new List<INamedTypeSymbol>();

            //interface list is small, we will not use HashSet here
            for (var t = type; t != null; t = t.BaseType)
            {
                foreach (var @interface in t.Interfaces)
                {
                    if (@interface.CanBeReferencedByName && interfaces.Contains(@interface) == false)
                    {
                        interfaces.Add(@interface);
                    }
                }
            }
        }

        if (attribute.SkipSystemInterfaces)
        {
            for (int i = 0; i < interfaces.Count; i++)
            {
                var @interface = interfaces[i];

                if (@interface.ContainingNamespace is INamespaceSymbol @namespace)
                {
                    if (@namespace.FirstNonGlobalNamespace() is INamespaceSymbol first)
                    {
                        if (first.Name.Equals("System", StringComparison.InvariantCulture) || first.Name.StartsWith("System.", StringComparison.InvariantCulture))
                        {
                            interfaces.RemoveAt(i--);
                        }
                    }
                }
            }
        }

        var options = SourceBuilderOptions.Load(context, null);
        var builder = new SourceBuilder(options);

        ICodeTextWriter writer = new CSharpCodeTextWriter(context, compilation);
        bool anyReasonToEmitSourceFile = false;
        bool error = false;

        builder.AppendLine("// <auto-generated />");
        //bool isNullable = compilation.Options.NullableContextOptions == NullableContextOptions.Enable;
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        writer.WriteNamespaceBeginning(builder, type.ContainingNamespace);

        List<INamedTypeSymbol> containingTypes = [];
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

        //type.TypeArguments
        //type.Name

        //sort interfaces

        if (interfaces.Any())
        {
            anyReasonToEmitSourceFile = true;

            string GetModifiedMethodName(string methodName)
            {
                if (methodName.StartsWith("I", StringComparison.InvariantCulture) && methodName.Length > 1 && char.IsUpper(methodName[1]))
                {
                    methodName = methodName.Substring(1);
                }
                return "As" + methodName;
            }

            void WriteMethod(INamedTypeSymbol @interface, int? index)
            {
                builder.AppendIndentation();
                builder.Append(@interface.DeclaredAccessibility == Accessibility.Internal ? "internal" : "public");
                builder.Append(' ');
                writer.WriteTypeReference(builder, @interface, scope);
                builder.Append(' ');
                builder.Append(GetModifiedMethodName(@interface.Name));
                //writer.WriteIdentifier(builder, @interface);
                if (index.HasValue)
                {
                    builder.Append('_');
                    builder.Append(index.Value);
                }
                builder.Append("() => ");
                writer.WriteHolderReference(builder, type, scope);
                builder.Append(';');
                builder.AppendLine();
            }

            foreach (var group in interfaces.GroupBy(i => i.Name))
            {
                if (group.Count() == 1)
                {
                    WriteMethod(group.First(), null);
                }
                else
                {
                    int index = 0;

                    foreach (var @interface in group.OrderBy(i => i.TypeArguments.Length).ThenBy(i => i.Name))
                    {
                        WriteMethod(@interface, index++);
                    }
                }
            }
        }

        builder.DecrementIndentation();
        builder.AppendIndentation();
        builder.Append('}');

        for (int i = 0; i < containingTypes.Count; i++)
        {
            builder.AppendLine();
            builder.DecrementIndentation();
            builder.AppendIndentation();
            builder.Append('}');
        }

        if (type.ContainingNamespace != null && type.ContainingNamespace.ConstituentNamespaces.Length > 0)
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

        return error == false && anyReasonToEmitSourceFile ? builder.ToString() : null;
    }

    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    private sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = [];

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any type with at least one attribute is a candidate for source generation
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax && typeDeclarationSyntax.AttributeLists.Count > 0)
            {
                this.Candidates.Add(typeDeclarationSyntax);
            }
        }
    }
}
