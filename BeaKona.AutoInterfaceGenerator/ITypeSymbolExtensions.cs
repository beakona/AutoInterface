using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BeaKona.AutoInterfaceGenerator;

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

    public static bool IsMemberImplementedExplicitly(this ITypeSymbol @this, ISymbol member)
    {
        ISymbol? implementation = @this.FindImplementationForInterfaceMember(member);

        if (implementation is null || implementation.IsStatic || !implementation.ContainingType.Equals(@this, SymbolEqualityComparer.Default))
        {
            return false;
        }

        return implementation switch
        {
            IMethodSymbol methodImplementation => methodImplementation.MethodKind == MethodKind.ExplicitInterfaceImplementation,
            IPropertySymbol propertyImplementation => propertyImplementation.ExplicitInterfaceImplementations.Any(),
            IEventSymbol eventImplementation => eventImplementation.ExplicitInterfaceImplementations.Any(),
            _ => false,
        };
    }

    //public static bool IsMemberImplemented(this ITypeSymbol @this, ISymbol member)
    //{
    //    ISymbol? implementation = @this.FindImplementationForInterfaceMember(member);

    //    return implementation is not null && implementation.IsStatic == false && implementation.ContainingType.Equals(@this, SymbolEqualityComparer.Default);
    //}

    public static ISymbol? FindImplementationForInterfaceMemberBySignature(this ITypeSymbol @this, ISymbol interfaceMember, bool strict)
    {
        string name = interfaceMember.Name;
        if (string.IsNullOrEmpty(name) == false)
        {
            foreach (ISymbol member in @this.GetMembers(name).Where(i => i.Kind == interfaceMember.Kind))
            {
                var comparer = new ComparerBySignature(strict);
                if (comparer.Equals(member, interfaceMember))
                {
                    return member;
                }
            }
        }

        return null;
    }

    public static bool IsMemberImplementedBySignature(this ITypeSymbol @this, ISymbol member, bool strict)
    {
        return @this.FindImplementationForInterfaceMemberBySignature(member, strict) != null;
    }

    public static bool IsAllInterfaceMembersImplementedBySignature(this ITypeSymbol @this, ITypeSymbol @interface, bool strict)
    {
        if (@interface.TypeKind != TypeKind.Interface)
        {
            return false;
        }

        foreach (var member in @interface.GetMembers())
        {
            if (@this.IsMemberImplementedBySignature(member, strict) == false)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsMatchByTypeOrImplementsInterface(this ITypeSymbol @this, ITypeSymbol @interface)
    {
        return @this.Equals(@interface, SymbolEqualityComparer.Default) || @this.AllInterfaces.Contains(@interface, SymbolEqualityComparer.Default);
    }

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

    public static INamedTypeSymbol[] GetContainingTypes(this ITypeSymbol @this)
    {
        List<INamedTypeSymbol> containingTypes = [];

        for (INamedTypeSymbol? ct = @this.ContainingType; ct != null; ct = ct.ContainingType)
        {
            containingTypes.Insert(0, ct);
        }

        return [.. containingTypes];
    }
}
