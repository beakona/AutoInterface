using System;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal class TemplateDefinition : IEquatable<TemplateDefinition>
    {
        public TemplateDefinition(string language, string body)
        {
            this.Language = language ?? "";
            this.Body = body ?? "";
        }

        public string Language { get; }
        public string Body { get; }

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
}
