namespace BeaKona.AutoInterfaceGenerator;

public static class Diagnostics
{
    public static void Report(GeneratorExecutionContext context, DiagnosticDescriptor diagnosticsDescriptor, SyntaxNode? node, params object?[] messageArgs)
    {
        Diagnostics.Report(context, diagnosticsDescriptor, node?.GetLocation(), messageArgs);
    }

    public static void Report(GeneratorExecutionContext context, DiagnosticDescriptor diagnosticsDescriptor, ISymbol? member, params object?[] messageArgs)
    {
        Diagnostics.Report(context, diagnosticsDescriptor, member != null && member.Locations.Length > 0 ? member.Locations[0] : null, messageArgs);
    }

    public static void Report(GeneratorExecutionContext context, DiagnosticDescriptor diagnosticsDescriptor, Location? location, params object?[] messageArgs)
    {
        context.ReportDiagnostic(Diagnostic.Create(diagnosticsDescriptor, location, messageArgs));
    }

    public static readonly DiagnosticDescriptor BKAG01 = new(
        id: "BKAG01",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG01_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG01_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG01_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG02 = new(
        id: "BKAG02",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG02_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG02_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG02_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG03 = new(
        id: "BKAG03",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG03_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG03_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG03_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG04 = new(
        id: "BKAG04",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG04_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG04_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG04_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG06 = new(
        id: "BKAG06",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG06_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG06_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG06_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG07 = new(
        id: "BKAG07",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG07_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG07_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG07_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG08 = new(
        id: "BKAG08",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG08_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG08_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG08_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG09 = new(
        id: "BKAG09",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG09_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG09_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG09_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG10 = new(
        id: "BKAG10",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG10_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG10_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG10_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG11 = new(
        id: "BKAG11",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG11_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG11_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG11_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG12 = new(
        id: "BKAG12",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG12_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG12_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG12_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG13 = new(
        id: "BKAG13",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG13_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG13_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG13_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG14 = new(
        id: "BKAG14",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG14_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG14_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG14_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG15 = new(
        id: "BKAG15",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG15_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG15_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG15_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG16 = new(
        id: "BKAG16",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG16_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG16_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG16_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );

    public static readonly DiagnosticDescriptor BKAG17 = new(
        id: "BKAG17",
        title: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG17_title), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        messageFormat: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG17_message), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(AutoInterfaceResource.BKAG17_description), AutoInterfaceResource.ResourceManager, typeof(AutoInterfaceResource)),
        helpLinkUri: "https://github.com/beakona/AutoInterface",
        customTags: WellKnownDiagnosticTags.NotConfigurable
    );
}
