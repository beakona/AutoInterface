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
}
