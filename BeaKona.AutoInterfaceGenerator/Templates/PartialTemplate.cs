using System.Text.RegularExpressions;

namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class PartialTemplate(AutoInterfaceTargets memberTargets, Regex? memberFilter, TemplateDefinition template)
{
    public AutoInterfaceTargets MemberTargets = memberTargets;
    public Regex? MemberFilter = memberFilter;
    public TemplateDefinition Template = template;

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
                        Diagnostics.Report(context, Diagnostics.BKAG16, ts[0].reference, name);
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
                        Diagnostics.Report(context, Diagnostics.BKAG16, ts[0].reference, name);
                        return null;
                    }
                }
            default:
                Diagnostics.Report(context, Diagnostics.BKAG16, ts[0].reference, name);
                return null;
        }
    }
}
