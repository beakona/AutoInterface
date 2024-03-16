using System.Collections;

namespace BeaKona.AutoInterfaceGenerator;

public sealed class AliasRegistry : IEnumerable<string>
{
    private readonly HashSet<string> aliases = [];

    public void Add(string alias) => this.aliases.Add(alias);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<string> GetEnumerator() => this.aliases.GetEnumerator();
}
