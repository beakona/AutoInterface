using System.Collections;

namespace BeaKona.AutoInterfaceGenerator;

public sealed class TypeRegistry : IEnumerable<INamedTypeSymbol>
{
    private readonly HashSet<INamedTypeSymbol> missingAttributeTypes = new(SymbolEqualityComparer.Default);
    public void Add(INamedTypeSymbol attributeType)
    {
        this.missingAttributeTypes.Add(attributeType);
    }

    public void AddMany(IEnumerable<INamedTypeSymbol> attributeTypes)
    {
        foreach (var attributeType in attributeTypes)
        {
            this.Add(attributeType);
        }
    }

    public IEnumerator<INamedTypeSymbol> GetEnumerator() => this.missingAttributeTypes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
