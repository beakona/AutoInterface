using System.Collections.Immutable;

namespace BeaKona.AutoInterfaceGenerator;

internal readonly struct AttributeDataWithTarget(AttributeData attribute, string? target)
{
    public AttributeData Attribute { get; } = attribute;
    public string? Target { get; } = target;

    public static readonly IEqualityComparer<AttributeDataWithTarget> AttributeComparer = new SimpleAttributeComparer();
    private sealed class SimpleAttributeComparer : IEqualityComparer<AttributeDataWithTarget>
    {
        public bool Equals(AttributeDataWithTarget x, AttributeDataWithTarget y) => x.Attribute.Equals(y.Attribute);

        public int GetHashCode(AttributeDataWithTarget obj) => obj.Attribute.GetHashCode();
    }

    public static readonly IEqualityComparer<AttributeDataWithTarget> DefaultComparer = new DefaultComparer2();
    private sealed class DefaultComparer2 : IEqualityComparer<AttributeDataWithTarget>
    {
        public bool Equals(AttributeDataWithTarget x, AttributeDataWithTarget y) => AttributeDataComparer.Equals(x.Attribute, y.Attribute) && Helpers.EqualStrings(x.Target, y.Target);

        public int GetHashCode(AttributeDataWithTarget obj)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj.Attribute.AttributeClass) + (obj.Target?.GetHashCode() ?? 0);
        }
    }

    private static readonly IEqualityComparer<TypedConstant> TypedConstantComparer = new TypedConstantComparer2();
    private sealed class TypedConstantComparer2 : IEqualityComparer<TypedConstant>
    {
        public bool Equals(TypedConstant x, TypedConstant y)
        {
            if (x.Kind != y.Kind)
            {
                return false;
            }

            if (x.IsNull)
            {
                if (y.IsNull)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (y.IsNull)
                {
                    return false;
                }
                else
                {
                    if (x.Value is ISymbol xs && y.Value is ISymbol ys)
                    {
                        return SymbolEqualityComparer.Default.Equals(xs, ys);
                    }
                    else
                    {
                        return object.Equals(x.Value, y.Value);
                    }
                }
            }
        }

        public int GetHashCode(TypedConstant obj)
        {
            return unchecked(obj.Kind.GetHashCode() + (obj.IsNull ? 0 : (obj.Value?.GetHashCode() ?? 0)));
        }
    }

    public static readonly IEqualityComparer<AttributeData> AttributeDataComparer = new AttributeDataComparer2();

    private sealed class AttributeDataComparer2 : IEqualityComparer<AttributeData>
    {
        public bool Equals(AttributeData x, AttributeData y)
        {
            if (SymbolEqualityComparer.Default.Equals(x.AttributeClass, y.AttributeClass) == false)
            {
                return false;
            }

            if (Helpers.EqualCollections(x.ConstructorArguments, y.ConstructorArguments, TypedConstantComparer) == false)
            {
                return false;
            }

            if (Helpers.EqualDictionaries(x.NamedArguments.ToImmutableDictionary(i => i.Key, i => i.Value), y.NamedArguments.ToImmutableDictionary(i => i.Key, i => i.Value), TypedConstantComparer) == false)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(AttributeData obj)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj.AttributeClass);
        }
    }
}
