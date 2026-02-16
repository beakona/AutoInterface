using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator;

internal static class Helpers
{
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

    public static bool EqualDictionaries<TKey, TValue>(ImmutableDictionary<TKey, TValue> x, ImmutableDictionary<TKey, TValue> y, IEqualityComparer<TValue>? comparer = null) where TKey : notnull
    {
        if (x.Count != y.Count)
        {
            return false;
        }

        comparer ??= EqualityComparer<TValue>.Default;

        foreach (var item in x)
        {
            if (y.TryGetValue(item.Key, out var yValue) == false)
            {
                return false;
            }

            var xValue = item.Value;

            if (comparer.Equals(xValue, yValue) == false)
            {
                return false;
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

    public static bool EqualStrings(string? s1, string? s2)
    {
        if (s1 != null)
        {
            if (s2 != null)
            {
                return string.Equals(s1, s2);
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (s2 != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public static bool EqualStrings(string? s1, string? s2, StringComparison comparisonType)
    {
        if (s1 != null)
        {
            if (s2 != null)
            {
                return string.Equals(s1, s2, comparisonType);
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (s2 != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
