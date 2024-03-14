namespace BeaKona.AutoInterfaceGenerator.Templates;

internal class TemplateDefinition(string language, string body) : IEquatable<TemplateDefinition>
{
    public string Language { get; } = language ?? "";
    public string Body { get; } = body ?? "";

    public bool Equals(TemplateDefinition other)
    {
        if (other == null)
        {
            return false;
        }

        if (this == other)
        {
            return true;
        }

        if (this.Language != other.Language)
        {
            return false;
        }

        if (this.Body != other.Body)
        {
            return false;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is TemplateDefinition o)
        {
            return this.Equals(o);
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return this.GetType().GetHashCode() + this.Language.GetHashCode() + this.Body.GetHashCode();
    }
}
