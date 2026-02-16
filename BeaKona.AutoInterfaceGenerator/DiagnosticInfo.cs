namespace BeaKona.AutoInterfaceGenerator;

internal sealed record class DiagnosticInfo(DiagnosticDescriptor Descriptor, Location? Location, params object?[] MessageArgs)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode? node, params object?[] messageArgs)
    {
        return new DiagnosticInfo(descriptor, node?.GetLocation(), messageArgs);
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol? symbol, params object?[] messageArgs)
    {
        return new DiagnosticInfo(descriptor, symbol != null && symbol.Locations.Length > 0 ? symbol.Locations[0] : null, messageArgs);
    }

    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, MessageArgs);
}
