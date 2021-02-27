using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator
{
    internal static class ITypeSymbolExtensions
    {
        public static IEnumerable<IMethodSymbol> GetMethods(this ITypeSymbol @this)
        {
            foreach (ISymbol member in @this.GetMembers())
            {
                if (member is IMethodSymbol method)
                {
                    if (method.MethodKind == MethodKind.Ordinary)
                    {
                        yield return method;
                    }
                }
            }
        }

        public static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol @this)
        {
            foreach (ISymbol member in @this.GetMembers())
            {
                if (member is IPropertySymbol property)
                {
                    if (property.IsIndexer == false)
                    {
                        yield return property;
                    }
                }
            }
        }

        public static IEnumerable<IPropertySymbol> GetIndexers(this ITypeSymbol @this)
        {
            foreach (ISymbol member in @this.GetMembers())
            {
                if (member is IPropertySymbol property)
                {
                    if (property.IsIndexer)
                    {
                        yield return property;
                    }
                }
            }
        }

        public static IEnumerable<IEventSymbol> GetEvents(this ITypeSymbol @this)
        {
            foreach (ISymbol member in @this.GetMembers())
            {
                if (member is IEventSymbol @event)
                {
                    yield return @event;
                }
            }
        }

        public static bool IsMemberImplemented(this ITypeSymbol @this, ISymbol member)
        {
            if (@this.FindImplementationForInterfaceMember(member) is IMethodSymbol memberImplementation)
            {
                return memberImplementation.ContainingType.Equals(@this, SymbolEqualityComparer.Default) && memberImplementation.MethodKind == MethodKind.ExplicitInterfaceImplementation;
            }

            return false;
        }
    }
}
