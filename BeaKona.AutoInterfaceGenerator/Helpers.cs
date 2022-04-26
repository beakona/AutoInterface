using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator;

public static class Helpers
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
        LocalizableString ltitle = new LocalizableResourceString(title, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
        LocalizableString lmessage = new LocalizableResourceString(message, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
        LocalizableString? ldescription = new LocalizableResourceString(description, AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource));
        string category = typeof(AutoInterfaceSourceGenerator).Namespace;
        string? link = "https://github.com/beakona/AutoInterface";
        DiagnosticDescriptor dd = new(id, ltitle, lmessage, category, severity, true, ldescription, link, WellKnownDiagnosticTags.NotConfigurable);
        Diagnostic d = Diagnostic.Create(dd, location, messageArgs);
        context.ReportDiagnostic(d);
    }

    public static bool EqualSets<T>(ImmutableArray<T> x, ImmutableArray<T> y, IEqualityComparer<T>? comparer = null)
    {
        if (x.IsEmpty)
        {
            return y.IsEmpty;
        }
        else
        {
            if (y.IsEmpty)
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
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            while(length > 0)
            {
                bool matched = false;
                for (int i = length-1; i >= 0; i--)
                {
                    if (comparer.Equals(x[length-1], y[i]))
                    {
                        matched = true;
                        x.RemoveAt(length-1);
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
        if (x.IsEmpty)
        {
            return y.IsEmpty;
        }
        else
        {
            if (y.IsEmpty)
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
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

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
}
