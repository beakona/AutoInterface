namespace BeaKona.AutoInterfaceGenerator
{
    internal interface ISourceTextGenerator
    {
        void Emit(ICodeTextWriter writer, SourceBuilder builder, object? model, ref bool separatorRequired);
    }
}
