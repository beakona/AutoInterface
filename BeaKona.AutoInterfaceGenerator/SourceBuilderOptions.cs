using System.Globalization;

namespace BeaKona.AutoInterfaceGenerator;

internal sealed class SourceBuilderOptions
{
    public string Indentation { get; set; } = "    ";
    public string NewLine { get; set; } = "\r\n";
    public bool InsertFinalNewLine { get; set; } = true;

    public static SourceBuilderOptions Load(GeneratorExecutionContext context, SyntaxTree? syntaxTree)
    {
        var builderOptions = new SourceBuilderOptions();

        var configOptions = syntaxTree != null ? context.AnalyzerConfigOptions.GetOptions(syntaxTree) : context.AnalyzerConfigOptions.GlobalOptions;

        var character = ' ';
        if (configOptions.TryGetValue("indent_style", out var indentStyle))
        {
            character = indentStyle.Equals("space", StringComparison.OrdinalIgnoreCase) ? ' ' : '\t';
        }

        var indentSize = 4;
        if (configOptions.TryGetValue("indent_size", out var indentSizeText) && int.TryParse(indentSizeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var indentSizeValue))
        {
            indentSize = indentSizeValue;
        }

        builderOptions.Indentation = new string(character, indentSize);

        if (configOptions.TryGetValue("end_of_line", out var endOfLine))
        {
            builderOptions.NewLine = (object)(endOfLine ?? "") switch
            {
                "" => "\r\n",
                "native" => "\r\n",
                "autocrlf" => "\r\n",
                "cr" => "\r",
                "lf" => "\n",
                "crlf" => "\r\n",
                "lfcr" => "\n\r",
                "nel" => "\u0085",
                _ => "\r\n",
            };
        }

        if (configOptions.TryGetValue("insert_final_newline", out var insertFinalNewline))
        {
            builderOptions.InsertFinalNewLine = insertFinalNewline.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        return builderOptions;
    }
}
