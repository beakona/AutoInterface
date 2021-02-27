namespace BeaKona.AutoInterfaceGenerator
{
    internal interface ISourceTextGenerator
    {
        void Emit(ICodeTextWriter writer, SourceBuilder builder, Model model, ref bool separatorRequired, out bool anyReasonToEmitSourceFile);
    }
}
