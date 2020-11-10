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
namespace BeaKona
{
    using System;
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class AutoInterfaceAttribute : Attribute
    {
        public AutoInterfaceAttribute(Type type)
        {
            this.Type = type;
        }
        public Type Type { get; }
    }
}
";

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
                        List<AutoInterfaceInfo> symbols = new List<AutoInterfaceInfo>();

                        void Process(ISymbol symbol, ITypeSymbol receiverType)
                        {
                            foreach (AttributeData attribute in symbol.GetAttributes())
                            {
                                if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                                {
                                    if (attribute.ConstructorArguments.Length > 0)
                                    {
                                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol type)
                                        {
                                            if (type.TypeKind == TypeKind.Interface)
                                            {
                                                if (receiverType.Equals(type, SymbolEqualityComparer.Default) || receiverType.AllInterfaces.Contains(type, SymbolEqualityComparer.Default))
                                                {
                                                    symbols.Add(new AutoInterfaceInfo(symbol, receiverType, attribute, type));
                                                }
                                                else
                                                {
                                                    ReportDiagnostic(context, "BK-AG04", nameof(AutoInterfaceResource.AG04_title), nameof(AutoInterfaceResource.AG04_message), nameof(AutoInterfaceResource.AG04_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                                        receiverType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                ReportDiagnostic(context, "BK-AG03", nameof(AutoInterfaceResource.AG03_title), nameof(AutoInterfaceResource.AG03_message), nameof(AutoInterfaceResource.AG03_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax(),
                                                    type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            ReportDiagnostic(context, "BK-AG07", nameof(AutoInterfaceResource.AG07_title), nameof(AutoInterfaceResource.AG07_message), nameof(AutoInterfaceResource.AG07_description), DiagnosticSeverity.Error, attribute.ApplicationSyntaxReference?.GetSyntax());
                                            break;
                                        }
                                    }
                                }
                            }
                        }

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
                                        Process(field, field.Type);
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
                                        break;
                                    }

                                    Process(property, property.Type);
                                }
                            }
                        }

                        CSharpBuildContext buildContext = new CSharpBuildContext(compilation);

                        // group the elements by class, and generate the source
                        foreach (IGrouping<INamedTypeSymbol, AutoInterfaceInfo> group in symbols.GroupBy(i => i.Member.ContainingType))
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

                            if (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct)
                            {
                            }
                            else
                            {
                                ReportDiagnostic(context, "BK-AG08", nameof(AutoInterfaceResource.AG08_title), nameof(AutoInterfaceResource.AG08_message), nameof(AutoInterfaceResource.AG08_description), DiagnosticSeverity.Error, group.First().Member);
                                continue;
                            }

                            List<AutoInterfaceInfo> itemsWithMissingInterface = group.Where(i => type.Interfaces.Contains(i.InterfaceType, SymbolEqualityComparer.Default) == false).ToList();

                            if (itemsWithMissingInterface.Count > 0)
                            {
                                HashSet<INamedTypeSymbol> emitted = new HashSet<INamedTypeSymbol>();
                                foreach (AutoInterfaceInfo itemWithMissingInterface in itemsWithMissingInterface)
                                {
                                    if (emitted.Add(itemWithMissingInterface.InterfaceType))
                                    {
                                        ReportDiagnostic(context, "BK-AG05", nameof(AutoInterfaceResource.AG05_title), nameof(AutoInterfaceResource.AG05_message), nameof(AutoInterfaceResource.AG05_description), DiagnosticSeverity.Error, itemWithMissingInterface.Member,
                                            itemWithMissingInterface.InterfaceType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                                    }
                                }

                                continue;
                            }

                            ISourceTextBuilder builder = new CSharpSourceTextBuilder(buildContext);
                            if (this.ProcessClass(builder, context, group.Key, group))
                            {
                                string name = group.Key.TypeArguments.Length > 0 ? $"{group.Key.Name}_{group.Key.TypeArguments.Length}" : group.Key.Name;
                                //GeneratePreview(context, name, builder.ToString());
                                context.AddSource($"{name}_AutoInterface.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
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

        private bool ProcessClass(ISourceTextBuilder builder, GeneratorExecutionContext context, INamedTypeSymbol type, IEnumerable<AutoInterfaceInfo> infos)
        {
            Compilation compilation = context.Compilation;

            ScopeInfo scope = new ScopeInfo(compilation, type);

            bool anyReasonToEmitSourceFile = false;

            //bool isNullable = compilation.Options.NullableContextOptions == NullableContextOptions.Enable;
            builder.AppendLine("#nullable enable");

            ImmutableArray<INamespaceSymbol> namespaceParts = type.ContainingNamespace != null && type.ContainingNamespace.IsGlobalNamespace == false ? builder.Context.GetNamespaceParts(type.ContainingNamespace) : new ImmutableArray<INamespaceSymbol>();
            if (namespaceParts.Length > 0)
            {
                builder.AppendNamespaceBegin(string.Join(".", namespaceParts.Select(i => builder.Context.GetSourceIdentifier(i))));
            }

            List<INamedTypeSymbol> containingTypes = new List<INamedTypeSymbol>();
            for (INamedTypeSymbol? ct = type.ContainingType; ct != null; ct = ct.ContainingType)
            {
                containingTypes.Insert(0, ct);
            }

            foreach (INamedTypeSymbol ct in containingTypes)
            {
                builder.AppendIndentation();
                builder.AppendTypeDeclarationBegin(ct, new ScopeInfo(compilation, ct));
                builder.AppendLine();
                builder.AppendIndentation();
                builder.AppendLine("{");
                builder.IncrementIndentation();
            }

            builder.AppendIndentation();
            builder.AppendTypeDeclarationBegin(type, scope);
            builder.AppendLine();
            builder.AppendIndentation();
            builder.AppendLine("{");
            builder.IncrementIndentation();

            bool separatorRequired = false;

            foreach (IGrouping<INamedTypeSymbol, AutoInterfaceInfo> group in infos.GroupBy(i => i.InterfaceType))
            {
                List<AutoInterfaceInfo> items = group.ToList();

                foreach (ISymbol member in group.Key.GetMembers())
                {
                    IMethodSymbol? memberImplementation = type.FindImplementationForInterfaceMember(member) as IMethodSymbol;
                    if (memberImplementation == null || memberImplementation.ContainingType.Equals(type, SymbolEqualityComparer.Default) == false || memberImplementation.MethodKind != MethodKind.ExplicitInterfaceImplementation)
                    {
                        if (member is IMethodSymbol method)
                        {
                            if (method.MethodKind == MethodKind.Ordinary)
                            {
                                anyReasonToEmitSourceFile = true;

                                if (separatorRequired)
                                {
                                    builder.AppendLine();
                                }
                                builder.AppendMethodDefinition(compilation, method, scope, group.Key, items);
                                separatorRequired = true;
                            }
                        }
                        else if (member is IPropertySymbol property)
                        {
                            anyReasonToEmitSourceFile = true;

                            if (separatorRequired)
                            {
                                builder.AppendLine();
                            }

                            builder.AppendPropertyDefinition(property, scope, group.Key, items);
                            separatorRequired = true;
                        }
                        else if (member is IEventSymbol @event)
                        {
                            anyReasonToEmitSourceFile = true;

                            if (separatorRequired)
                            {
                                builder.AppendLine();
                            }

                            builder.AppendEventDefinition(@event, scope, group.Key, items);
                            separatorRequired = true;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }

            builder.DecrementIndentation();
            builder.AppendIndentation();
            builder.AppendLine("}");

            for (int i = 0; i < containingTypes.Count; i++)
            {
                builder.DecrementIndentation();
                builder.AppendIndentation();
                builder.AppendLine("}");
            }

            if (namespaceParts.Length > 0)
            {
                builder.DecrementIndentation();
                builder.AppendIndentation();
                builder.AppendLine("}");
            }

            return anyReasonToEmitSourceFile;
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
            string? link = null;
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