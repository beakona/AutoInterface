using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace BeaKona.AutoInterfaceGenerator
{
    internal static class INamedTypeSymbolExtensions
    {
        public static bool IsPartial(this ITypeSymbol @this)
        {
            foreach (SyntaxReference syntax in @this.DeclaringSyntaxReferences)
            {
                if (syntax.GetSyntax() is MemberDeclarationSyntax declaration)
                {
                    if (declaration.Modifiers.Any(i => i.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
