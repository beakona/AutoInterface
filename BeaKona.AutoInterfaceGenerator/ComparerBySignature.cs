namespace BeaKona.AutoInterfaceGenerator;

public sealed class ComparerBySignature : IEqualityComparer<ISymbol>
{
    private readonly Dictionary<ITypeParameterSymbol, List<ITypeParameterSymbol>> aliasesByKey = new(SymbolEqualityComparer.Default);

    public int GetHashCode(ISymbol obj) => throw new NotSupportedException();//SymbolEqualityComparer.Default.GetHashCode(obj);

    public bool Equals(ISymbol s1, ISymbol s2)
    {
        if (s1.Kind == s2.Kind)
        {
            if (s1.Kind == SymbolKind.Method)
            {
                IMethodSymbol v1 = (IMethodSymbol)s1;
                IMethodSymbol v2 = (IMethodSymbol)s2;

                if (v1.Arity == v2.Arity)
                {
                    List<(ITypeParameterSymbol t1, ITypeParameterSymbol t2)>? toRemove = null;

                    bool match = true;

                    if (v1.Arity > 0)
                    {
                        for (int i = 0; i < v1.Arity; i++)
                        {
                            ITypeParameterSymbol t1 = (ITypeParameterSymbol)v1.TypeArguments[i];
                            ITypeParameterSymbol t2 = (ITypeParameterSymbol)v2.TypeArguments[i];

                            if (t1.HasConstructorConstraint != t2.HasConstructorConstraint
                                || t1.HasNotNullConstraint != t2.HasNotNullConstraint
                                || t1.HasReferenceTypeConstraint != t2.HasReferenceTypeConstraint
                                || t1.HasUnmanagedTypeConstraint != t2.HasUnmanagedTypeConstraint
                                || t1.HasValueTypeConstraint != t2.HasValueTypeConstraint
                                || t1.ReferenceTypeConstraintNullableAnnotation != t2.ReferenceTypeConstraintNullableAnnotation)
                            {
                                match = false;
                                break;
                            }

                            if (Helpers.EqualSets(t1.ConstraintTypes, t2.ConstraintTypes, this) == false)
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            toRemove = [];

                            for (int i = 0; i < v1.Arity; i++)
                            {
                                ITypeParameterSymbol t1 = (ITypeParameterSymbol)v1.TypeArguments[i];
                                ITypeParameterSymbol t2 = (ITypeParameterSymbol)v2.TypeArguments[i];

                                if (this.aliasesByKey.TryGetValue(t1, out var aliases) == false)
                                {
                                    this.aliasesByKey[t1] = aliases = [];
                                }
                                aliases.Add(t2);

                                toRemove.Add((t1, t2));
                            }
                        }
                    }

                    try
                    {
                        if (match)
                        {
                            if (this.Equals(v1.ReturnType, v2.ReturnType) && v1.ReturnsByRefReadonly == v2.ReturnsByRefReadonly && v1.ReturnsByRef == v2.ReturnsByRef)
                            {
                                if (Helpers.EqualCollections(v1.Parameters, v2.Parameters, this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (toRemove != null)
                        {
                            foreach ((ITypeParameterSymbol t1, ITypeParameterSymbol t2) in toRemove)
                            {
                                if (this.aliasesByKey.TryGetValue(t1, out var aliases))
                                {
                                    aliases.Remove(t2);
                                    if (aliases.Count == 0)
                                    {
                                        this.aliasesByKey.Remove(t1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (s1.Kind == SymbolKind.Property)
            {
                IPropertySymbol v1 = (IPropertySymbol)s1;
                IPropertySymbol v2 = (IPropertySymbol)s2;
                if (v1.IsIndexer == v2.IsIndexer)
                {
                    if (this.Equals(v1.Type, v2.Type))
                    {
                        static bool HasGetter(IPropertySymbol p) => p.GetMethod != null;
                        static bool HasSetter(IPropertySymbol p) => p.SetMethod != null;
                        if (HasGetter(v1) == HasGetter(v2) && HasSetter(v1) == HasSetter(v2))
                        {
                            if (v1.IsIndexer)
                            {
                                return Helpers.EqualCollections(v1.Parameters, v2.Parameters, this);
                                //v1.RefCustomModifiers
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (s1.Kind == SymbolKind.Event)
            {
                IEventSymbol v1 = (IEventSymbol)s1;
                IEventSymbol v2 = (IEventSymbol)s2;
                return this.Equals(v1.Type, v2.Type);
            }
            else if (s1.Kind == SymbolKind.Field)
            {
                IFieldSymbol v1 = (IFieldSymbol)s1;
                IFieldSymbol v2 = (IFieldSymbol)s2;
                if (this.Equals(v1.Type, v2.Type))
                {
                    return v1.IsReadOnly == v2.IsReadOnly && v1.IsVolatile == v2.IsVolatile && v1.IsConst == v2.IsConst;
                }
            }
            else if (s1.Kind == SymbolKind.Parameter)
            {
                IParameterSymbol v1 = (IParameterSymbol)s1;
                IParameterSymbol v2 = (IParameterSymbol)s2;
                if (this.Equals(v1.Type, v2.Type))
                {
                    return v1.RefKind == v2.RefKind && v1.IsParams == v2.IsParams && v1.IsOptional == v2.IsOptional && v1.IsThis == v2.IsThis /*&& v1.IsNullChecked == v2.IsNullChecked*/;
                }
            }
            else if (s1.Kind == SymbolKind.TypeParameter)
            {
                if (s1.Equals(s2, SymbolEqualityComparer.Default))
                {
                    return true;
                }
                else
                {
                    if (s1 is ITypeParameterSymbol p1)
                    {
                        if (this.aliasesByKey.TryGetValue(p1, out var aliases))
                        {
                            if (aliases.Any(i => i.Equals(s2, SymbolEqualityComparer.Default)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (s1.Kind == SymbolKind.NamedType)
            {
                if (s1.Equals(s2, SymbolEqualityComparer.Default))
                {
                    return true;
                }
                else
                {
                    INamedTypeSymbol n1 = (INamedTypeSymbol)s1;
                    INamedTypeSymbol n2 = (INamedTypeSymbol)s2;
                    if (n1.IsGenericType && n2.IsGenericType)
                    {
                        if (n1.OriginalDefinition.Equals(n2.OriginalDefinition, SymbolEqualityComparer.Default))
                        {
                            //var definitionTypeParameter = n1.OriginalDefinition.TypeParameters[0]; //example:T from interface definition
                            //var definitionTypeArgument = n1.OriginalDefinition.TypeArguments[0]; //example: same as definitionTypeParameter because it is definition
                            //var typeParameter = n1.TypeParameters[0];//example: T from target method
                            //var typeArgument = n1.TypeArguments[0];//example: int -> concrete type argument

                            if (Helpers.EqualCollections(n1.TypeArguments, n2.TypeArguments, this))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (s1.Kind == SymbolKind.ArrayType)
            {
                IArrayTypeSymbol a1 = (IArrayTypeSymbol)s1;
                IArrayTypeSymbol a2 = (IArrayTypeSymbol)s2;

                if (a1.Rank == a2.Rank)
                {
                    if (Helpers.EqualCollections(a1.Sizes, a2.Sizes))
                    {
                        if (Helpers.EqualCollections(a1.LowerBounds, a2.LowerBounds))
                        {
                            if (this.Equals(a1.ElementType, a2.ElementType))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (s1.Kind == SymbolKind.PointerType)
            {
                IPointerTypeSymbol p1 = (IPointerTypeSymbol)s1;
                IPointerTypeSymbol p2 = (IPointerTypeSymbol)s2;

                if (this.Equals(p1.PointedAtType, p2.PointedAtType))
                {
                    return true;
                }
            }
            else if (s1.Kind == SymbolKind.FunctionPointerType)
            {
                IFunctionPointerTypeSymbol p1 = (IFunctionPointerTypeSymbol)s1;
                IFunctionPointerTypeSymbol p2 = (IFunctionPointerTypeSymbol)s2;

                if (this.Equals(p1.Signature, p2.Signature))
                {
                    return true;
                }
            }
            else if (s1.Kind == SymbolKind.DynamicType)
            {
                return true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        return false;
    }
}
