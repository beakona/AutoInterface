using System;

namespace BeaKona.AutoInterfaceGenerator
{
    internal class SourceBuilderOptions
    {
        public string Identation { get; set; } = "  ";
        public string NewLine { get; set; } = Environment.NewLine;

        public static SourceBuilderOptions Default { get; } = new SourceBuilderOptions();
    }
}
