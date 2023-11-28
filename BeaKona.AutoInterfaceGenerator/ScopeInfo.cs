using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BeaKona.AutoInterfaceGenerator;

internal sealed class ScopeInfo
{
    public ScopeInfo(ITypeSymbol type)
    {
        this.Type = type;
        this.usedTypeArguments = new HashSet<string>(ScopeInfo.AllTypeArguments(type).Select(i => i.Name));
    }

    public ScopeInfo(ScopeInfo parentScope)
    {
        this.Type = parentScope.Type;
        this.usedTypeArguments = new HashSet<string>(parentScope.usedTypeArguments);
    }

    public ITypeSymbol Type { get; }

    private readonly HashSet<string> usedTypeArguments;
    private readonly Dictionary<ITypeSymbol, string> aliasTypeParameterNameByCanonicalType = new(SymbolEqualityComparer.Default);

    private static ImmutableList<ITypeSymbol> AllTypeArguments(ISymbol symbol)
    {
        List<ITypeSymbol> types = [];

        for (ISymbol s = symbol; s != null; s = s.ContainingType)
        {
            if (s is INamedTypeSymbol ts)
            {
                types.AddRange(ts.TypeArguments);
            }
            else if (s is IMethodSymbol m)
            {
                types.AddRange(m.TypeArguments);
            }
        }

        return types.ToImmutableList();
    }

    public void CreateAliases(ImmutableArray<ITypeSymbol> typeArguments)
    {
        foreach (ITypeSymbol t in typeArguments)
        {
            if (this.aliasTypeParameterNameByCanonicalType.ContainsKey(t))
            {
                throw new InvalidOperationException();
            }

            if (this.usedTypeArguments.Contains(t.Name))
            {
                if (ScopeInfo.TrySplitAsBaseNameAndInteger(t.Name, out string? baseName, out int? index) == false)
                {
                    baseName = t.Name;
                    index = 0;
                }
                string typeName;
                do
                {
                    typeName = $"{baseName}{++index}";
                }
                while (this.usedTypeArguments.Contains(typeName));
                this.aliasTypeParameterNameByCanonicalType.Add(t, typeName);
            }
        }
    }

    public bool TryGetAlias(ITypeSymbol symbol, /*[NotNullWhen(true)]*/ out string? alias)
    {
        return this.aliasTypeParameterNameByCanonicalType.TryGetValue(symbol, out alias);
    }

    private static readonly Regex rxSplitter = new(@"^\s*(?<n>\w+)(?<value>\d+)\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static bool TrySplitAsBaseNameAndInteger(string name, /*[NotNullWhen(true)]*/ out string? baseName, /*[NotNullWhen(true)]*/ out int? value)
    {
        Match m = rxSplitter.Match(name);
        if (m.Success)
        {
            baseName = m.Groups["n"].Value;
            value = int.Parse(m.Groups["v"].Value, CultureInfo.InvariantCulture);
            return true;
        }
        else
        {
            baseName = null;
            value = null;
            return false;
        }
    }
}
