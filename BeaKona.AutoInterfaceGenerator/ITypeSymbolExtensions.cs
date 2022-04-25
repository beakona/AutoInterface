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

    public static bool IsMemberImplemented(this ITypeSymbol @this, ISymbol member)
    {
        if (@this.FindImplementationForInterfaceMember(member) is IMethodSymbol memberImplementation)
        {
            return memberImplementation.ContainingType.Equals(@this, SymbolEqualityComparer.Default) && memberImplementation.MethodKind == MethodKind.ExplicitInterfaceImplementation;
        }

        return false;
    }

    private static bool SameType(ITypeSymbol? t1, NullableAnnotation a1, ITypeSymbol? t2, NullableAnnotation a2)
    {
        if (t1 == null)
        {
            return t2 == null;
        }
        else
        {
            if (t2 == null)
            {
                return false;
            }
            else
            {
                return a1 == a2 && t1.Equals(t2, SymbolEqualityComparer.Default);
            }
        }
    }

    private static bool SameSignature<T>(IList<T> parameters1, IList<T> parameters2) where T: IParameterSymbol
    {
        int length = parameters1.Count;
        if (length != parameters2.Count)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (SameSignature(parameters1[i], parameters2[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameSignature(IList<ITypeSymbol> types1, IList<NullableAnnotation> annotations1, IList<ITypeSymbol> types2, IList<NullableAnnotation> annotations2)
    {
        int length = types1.Count;
        if (length != types2.Count || length != annotations1.Count || length != annotations2.Count)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (SameType(types1[i], annotations1[i], types2[i], annotations2[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameSignature(ISymbol s1, ISymbol s2)
    {
        if (s1.Kind == s2.Kind)
        {
            if (s1.Kind == SymbolKind.Method)
            {
                IMethodSymbol v1 = (IMethodSymbol)s1;
                IMethodSymbol v2 = (IMethodSymbol)s2;

                if (SameType(v1.ReturnType, v1.ReturnNullableAnnotation, v2.ReturnType, v2.ReturnNullableAnnotation) && v1.ReturnsByRefReadonly == v2.ReturnsByRefReadonly && v1.ReturnsByRef == v2.ReturnsByRef)
                {
                    if (SameSignature(v1.Parameters, v2.Parameters))
                    {
                        if (v1.Arity == v2.Arity)
                        {
                            if (v1.Arity > 0)
                            {
                                return SameSignature(v1.TypeArguments, v1.TypeArgumentNullableAnnotations, v2.TypeArguments, v2.TypeArgumentNullableAnnotations);
                            }
                            else
                            {
                                return true;
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
                    if (SameType(v1.Type, v1.NullableAnnotation, v2.Type, v2.NullableAnnotation))
                    {
                        static bool HasGetter(IPropertySymbol p) => p.GetMethod != null;
                        static bool HasSetter(IPropertySymbol p) => p.SetMethod != null;
                        if (HasGetter(v1) == HasGetter(v2) && HasSetter(v1) == HasSetter(v2))
                        {
                            if (v1.IsIndexer)
                            {
                                return SameSignature(v1.Parameters, v2.Parameters);
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
                return SameType(v1.Type, v1.NullableAnnotation, v2.Type, v2.NullableAnnotation);
            }
            else if (s1.Kind == SymbolKind.Field)
            {
                IFieldSymbol v1 = (IFieldSymbol)s1;
                IFieldSymbol v2 = (IFieldSymbol)s2;
                if (SameType(v1.Type, v1.NullableAnnotation, v2.Type, v2.NullableAnnotation))
                {
                    return v1.IsReadOnly == v2.IsReadOnly && v1.IsVolatile == v2.IsVolatile && v1.IsConst == v2.IsConst;
                }
            }
            else if (s1.Kind == SymbolKind.Parameter)
            {
                IParameterSymbol v1 = (IParameterSymbol)s1;
                IParameterSymbol v2 = (IParameterSymbol)s2;
                if (SameType(v1.Type, v1.NullableAnnotation, v2.Type, v2.NullableAnnotation))
                {
                    return v1.RefKind == v2.RefKind && v1.IsParams == v2.IsParams && v1.IsOptional == v2.IsOptional && v1.IsThis == v2.IsThis && v1.IsNullChecked == v2.IsNullChecked;
                }
            }
        }

        return false;
    }

    public static ISymbol? FindImplementationForInterfaceMemberBySignature(this ITypeSymbol @this, ISymbol interfaceMember)
    {
        string name = interfaceMember.Name;
        if (string.IsNullOrEmpty(name) == false)
        {
            foreach (ISymbol member in @this.GetMembers(name).Where(i => i.Kind == interfaceMember.Kind))
            {
                if (SameSignature(member, interfaceMember))
                {
                    return member;
                }
            }
        }

        return null;
    }

    public static bool IsMemberImplementedBySignature(this ITypeSymbol @this, ISymbol member)
    {
        return @this.FindImplementationForInterfaceMemberBySignature(member) != null;
    }

    public static bool IsAllInterfaceMembersImplementedBySignature(this ITypeSymbol @this, ITypeSymbol @interface)
    {
        if (@interface.TypeKind != TypeKind.Interface)
        {
            return false;
        }

        foreach (ISymbol member in @interface.GetMembers())
        {
            if (@this.IsMemberImplementedBySignature(member) == false)
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
}
