using System.Text.RegularExpressions;

namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class PartialTemplate
{
    public PartialTemplate(AutoInterfaceTargets memberTargets, Regex? memberFilter, TemplateDefinition template)
    {
        this.MemberTargets = memberTargets;
        this.MemberFilter = memberFilter;
        this.Template = template;
    }

    public AutoInterfaceTargets MemberTargets;
    public Regex? MemberFilter;
    public TemplateDefinition Template;

    public bool FilterMatch(string name) => this.MemberFilter == null || this.MemberFilter.IsMatch(name);

    public static PartialTemplate? PickTemplate(GeneratorExecutionContext context, IEnumerable<(PartialTemplate template, ISymbol reference)> templates, string name)
    {
        (PartialTemplate template, ISymbol reference)[] ts = templates.ToArray();

        switch (ts.Length)
        {
            case 0: return null;
            case 1: return ts[0].template;
            case 2:
                if (ts[0].template.MemberFilter != null)
                {
                    if (ts[1].template.MemberFilter != null)
                    {
                        Helpers.ReportDiagnostic(context, "BKAG16", nameof(AutoInterfaceResource.AG16_title), nameof(AutoInterfaceResource.AG16_message), nameof(AutoInterfaceResource.AG16_description), DiagnosticSeverity.Error, ts[0].reference,
                            name);
                        return null;
                    }
                    else
                    {
                        return ts[0].template;
                    }
                }
                else
                {
                    if (ts[1].template.MemberFilter != null)
                    {
                        return ts[1].template;
                    }
                    else
                    {
                        Helpers.ReportDiagnostic(context, "BKAG16", nameof(AutoInterfaceResource.AG16_title), nameof(AutoInterfaceResource.AG16_message), nameof(AutoInterfaceResource.AG16_description), DiagnosticSeverity.Error, ts[0].reference,
                            name);
                        return null;
                    }
                }
            default:
                Helpers.ReportDiagnostic(context, "BKAG16", nameof(AutoInterfaceResource.AG16_title), nameof(AutoInterfaceResource.AG16_message), nameof(AutoInterfaceResource.AG16_description), DiagnosticSeverity.Error, ts[0].reference,
                    name);
                return null;
        }
    }
}
