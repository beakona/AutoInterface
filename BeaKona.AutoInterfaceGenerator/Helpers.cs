using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator;

internal static class Helpers
{
    public static void ReportDiagnostic(GeneratorExecutionContext context, string id, string title, string message, string description, DiagnosticSeverity severity, SyntaxNode? node, params object?[] messageArgs)
    {
        Helpers.ReportDiagnostic(context, id, title, message, description, severity, node?.GetLocation(), messageArgs);
    }

    public static void ReportDiagnostic(GeneratorExecutionContext context, string id, string title, string message, string description, DiagnosticSeverity severity, ISymbol? member, params object?[] messageArgs)
    {
        Helpers.ReportDiagnostic(context, id, title, message, description, severity, member != null && member.Locations.Length > 0 ? member.Locations[0] : null, messageArgs);
    }

    public static void ReportDiagnostic(GeneratorExecutionContext context, string id, string title, string message, string description, DiagnosticSeverity severity, Location? location, params object?[] messageArgs)
    {
        var lTitle = new LocalizableResourceString(title, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
        var lMessage = new LocalizableResourceString(message, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
        var lDescription = new LocalizableResourceString(description, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
        var category = typeof(AutoInterfaceSourceGenerator).Namespace;
        var link = "https://github.com/beakona/AutoInterface";

        var dd = new DiagnosticDescriptor(id, lTitle, lMessage, category, severity, true, lDescription, link, WellKnownDiagnosticTags.NotConfigurable);
        var d = Diagnostic.Create(dd, location, messageArgs);
        context.ReportDiagnostic(d);
    }

    public static bool EqualSets<T>(ImmutableArray<T> x, ImmutableArray<T> y, IEqualityComparer<T>? comparer = null)
    {
        if (x.IsDefaultOrEmpty)
        {
            return y.IsDefaultOrEmpty;
        }
        else
        {
            if (y.IsDefaultOrEmpty)
            {
                return false;
            }
            else
            {
                return Helpers.EqualSets((IList<T>)x, (IList<T>)y, comparer);
            }
        }
    }

    public static bool EqualSets<T>(IList<T> x, IList<T> y, IEqualityComparer<T>? comparer = null)
    {
        x = x.ToList();
        y = y.ToList();

        int length = x.Count;
        if (length != y.Count)
        {
            return false;
        }

        if (length > 0)
        {
            comparer ??= EqualityComparer<T>.Default;

            while (length > 0)
            {
                bool matched = false;
                for (int i = length - 1; i >= 0; i--)
                {
                    if (comparer.Equals(x[length - 1], y[i]))
                    {
                        matched = true;
                        x.RemoveAt(length - 1);
                        y.RemoveAt(i);
                        length--;
                        break;
                    }
                }
                if (!matched)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool EqualCollections<T>(ImmutableArray<T> x, ImmutableArray<T> y, IEqualityComparer<T>? comparer = null)
    {
        if (x.IsDefaultOrEmpty)
        {
            return y.IsDefaultOrEmpty;
        }
        else
        {
            if (y.IsDefaultOrEmpty)
            {
                return false;
            }
            else
            {
                return Helpers.EqualCollections((IList<T>)x, (IList<T>)y, comparer);
            }
        }
    }

    public static bool EqualCollections<T>(IList<T> x, IList<T> y, IEqualityComparer<T>? comparer = null)
    {
        int length = x.Count;
        if (length != y.Count)
        {
            return false;
        }

        if (length > 0)
        {
            comparer ??= EqualityComparer<T>.Default;

            for (int i = 0; i < length; i++)
            {
                if (comparer.Equals(x[i], y[i]) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool IsDynamic(IParameterSymbol symbol)
    {
        return Helpers.IsDynamic(symbol.Type);
    }

    public static bool IsDynamic(ITypeSymbol symbol)
    {
        return symbol.TypeKind == TypeKind.Dynamic;
    }

    public static bool HasOutParameters(IMethodSymbol method)
    {
        return Helpers.HasOutParameters(method.Parameters);
    }

    public static bool HasOutParameters(IEnumerable<IParameterSymbol> parameters)
    {
        foreach (IParameterSymbol parameter in parameters)
        {
            if (parameter.RefKind == RefKind.Out)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsPublicAccess(ITypeSymbol type)
    {
        if (type.DeclaredAccessibility == Accessibility.Public)
        {
            if (type is INamedTypeSymbol namedType)
            {
                return namedType.TypeArguments.All(Helpers.IsPublicAccess);
            }
            else
            {
                return true;
            }
        }

        return false;
    }
}
