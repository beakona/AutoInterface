using BeaKona.AutoInterfaceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

namespace BeaKona.AutoInterfaceGeneratorTest
{
    public class GeneratorTests
    {
        [Fact]
        public void SimpleGeneratorTest()
        {
            string source = @"
namespace MyTest
{
    public class Program
    {
        public static void Main()
        {
        }
    }
}
";

            Compilation comp = CreateCompilation(source);
            Compilation newComp = RunGenerators(comp, out ImmutableArray<Diagnostic> generatorDiags, new AutoInterfaceSourceGenerator());
            IEnumerable<SyntaxTree> generatedTrees = newComp.RemoveSyntaxTrees(comp.SyntaxTrees).SyntaxTrees;

            Assert.Single(generatedTrees);
            Assert.Empty(generatorDiags);
            Assert.Empty(newComp.GetDiagnostics());
        }

        private static Compilation CreateCompilation(string source, OutputKind outputKind = OutputKind.ConsoleApplication)
                  => CSharpCompilation.Create("compilation",
                      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) },
                      new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                      new CSharpCompilationOptions(outputKind));

        private static GeneratorDriver CreateDriver(Compilation c, params ISourceGenerator[] generators) => CSharpGeneratorDriver.Create(generators, parseOptions: (CSharpParseOptions)c.SyntaxTrees.First().Options);

        private static Compilation RunGenerators(Compilation c, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(c, generators).RunGeneratorsAndUpdateCompilation(c, out var d, out diagnostics);
            return d;
        }
    }
}