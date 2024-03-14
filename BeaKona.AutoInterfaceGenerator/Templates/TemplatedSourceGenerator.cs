﻿namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class TemplatedSourceTextGenerator(TemplateDefinition template) : ISourceTextGenerator
{
    public TemplateDefinition Template { get; } = template;

    public void Emit(ICodeTextWriter writer, SourceBuilder builder, object? model, ref bool separatorRequired)
    {
        var template = this.ResolveTemplate(this.Template.Language ?? "") ?? throw new NotSupportedException($"Template language '{this.Template.Language}' is not supported.");

        string text = template.Render(model);

        string[] lines = text.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.None);

        static bool HasNoContent(string line) => line.Trim().Length <= 0;

        foreach (string line in lines.SkipWhile(HasNoContent).Reverse().SkipWhile(HasNoContent).Reverse())
        {
            if (separatorRequired)
            {
                builder.AppendLine();
                separatorRequired = false;
            }

            if (line.Trim().Length > 0)
            {
                builder.AppendIndentation();
                builder.Append(line.TrimEnd());
                separatorRequired = true;
            }
            else
            {
                separatorRequired = true;
            }
        }
    }

    private Scriban.Template? ResolveTemplate(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "scriban" => Scriban.Template.Parse(this.Template.Body),
            "liquid" => Scriban.Template.ParseLiquid(this.Template.Body),
            _ => null,
        };
    }
}
