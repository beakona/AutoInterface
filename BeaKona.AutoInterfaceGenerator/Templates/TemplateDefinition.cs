﻿namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal class TemplateDefinition
    {
        public TemplateDefinition(string language, string body)
        {
            this.Language = language ?? "";
            this.Body = body ?? "";
        }

        public string Language { get; }
        public string Body { get; }

        public override bool Equals(object obj)
        {
            if (obj is TemplateDefinition o)
            {
                if (this == o)
                {
                    return true;
                }

                if (this.Language != o.Language)
                {
                    return false;
                }

                if (this.Body != o.Body)
                {
                    return false;
                }

                return true;
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
