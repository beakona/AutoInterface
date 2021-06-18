using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator
{
    internal static class SemanticFacts
    {
        public static string? ResolveAssemblyAlias(Compilation compilation, IAssemblySymbol assembly)
        {
            //MetadataReferenceProperties.GlobalAlias
            if (compilation.GetMetadataReference(assembly) is MetadataReference mr)
            {
                foreach (string alias in mr.Properties.Aliases)
                {
                    if (string.IsNullOrEmpty(alias) == false)
                    {
                        return alias;
                    }
                }
            }

            return null;
        }

        public static ISymbol[] GetRelativeSymbols(ITypeSymbol type, ITypeSymbol scope)
        {
            ISymbol[] typeSymbols = GetContainingSymbols(type, false);
            ISymbol[] scopeSymbols = GetContainingSymbols(scope, true);

            int count = Math.Min(typeSymbols.Length, scopeSymbols.Length);
            for (int i = 0; i < count; i++)
            {
                if (typeSymbols[i].Equals(scopeSymbols[i], SymbolEqualityComparer.Default) == false)
                {
                    int remaining = typeSymbols.Length - i;
                    if (remaining > 0)
                    {
                        ISymbol[] result = new ISymbol[remaining];
                        Array.Copy(typeSymbols, i, result, 0, result.Length);
                        return result;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return new ISymbol[0];
        }

        public static ISymbol[] GetContainingSymbols(ITypeSymbol type, bool includeSelf)
        {
            List<ISymbol> symbols = new();

            for (ISymbol t = includeSelf ? type : type.ContainingSymbol; t != null; t = t.ContainingSymbol)
            {
                if (t is IModuleSymbol || t is IAssemblySymbol)
                {
                    break;
                }
                if (t is INamespaceSymbol tn && tn.IsGlobalNamespace)
                {
                    break;
                }
                symbols.Insert(0, t);
            }

            return symbols.ToArray();
        }

        internal static (bool isAsync, bool returnsValue) IsAsyncAndGetReturnType(Compilation compilation, IMethodSymbol method)
        {
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
                        returnsValue = false;
                    }
                }
            }
            if (isAsync == false)
            {
                returnsValue = method.ReturnsVoid == false;
            }

            return (isAsync, returnsValue);
        }

        public static bool IsNullable(Compilation compilation, ITypeSymbol type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return true;
            }
            else if (type is INamedTypeSymbol namedType)
            {
                return SemanticFacts.IsNullableT(compilation, namedType);
            }

            return false;
        }

        public static bool IsNullableT(Compilation compilation, INamedTypeSymbol type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsValueType)
            {
                if (type.IsGenericType && type.IsUnboundGenericType == false)
                {
                    INamedTypeSymbol symbolNullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T);

                    return symbolNullableT.ConstructUnboundGenericType().Equals(type.ConstructUnboundGenericType(), SymbolEqualityComparer.Default);
                }
            }

            return false;
        }
    }
}
