using System;
using System.Diagnostics;

namespace BeaKona;

[Conditional("CodeGeneration")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class AutoInterfaceAttribute : Attribute
{
    public AutoInterfaceAttribute()
    {
    }

    public AutoInterfaceAttribute(Type type)
    {
        this.Type = type;
    }

    public Type? Type { get; }
    public bool PreferCoalesce { get; set; }
    public bool IncludeBaseInterfaces { get; set; }
    public string? TemplateLanguage { get; set; }
    public string? TemplateBody { get; set; }
    public string? TemplateFileName { get; set; }
    public bool AllowMissingMembers { get; set; }
}
