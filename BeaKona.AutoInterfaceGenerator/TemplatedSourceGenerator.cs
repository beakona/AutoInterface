using System;
using System.Linq;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class TemplatedSourceTextGenerator : ISourceTextGenerator
    {
        public TemplatedSourceTextGenerator(TemplateSettings settings)
        {
            this.Settings = settings;
        }

        public TemplateSettings Settings { get; }

        public void Emit(ICodeTextWriter writer, SourceBuilder builder, Model model, ref bool separatorRequired, out bool anyReasonToEmitSourceFile)
        {
            Scriban.Template? template = (this.Settings.Language ?? "").ToLowerInvariant() switch
            {
                "scriban" => Scriban.Template.Parse(this.Settings.Body),
                "liquid" => Scriban.Template.ParseLiquid(this.Settings.Body),
                _ => null,
            };

            if (template == null)
            {
                throw new NotSupportedException($"Template language '{this.Settings.Language}' is not supported.");
            }

            string text = template.Render(model);

            anyReasonToEmitSourceFile = true;

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
    }
}
