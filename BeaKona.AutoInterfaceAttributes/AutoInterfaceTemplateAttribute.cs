using System;
using System.Diagnostics;

namespace BeaKona;

[Conditional("CodeGeneration")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class AutoInterfaceTemplateAttribute : Attribute
{
    public AutoInterfaceTemplateAttribute(AutoInterfaceTargets targets)
    {
        this.Targets = targets;
    }

    public AutoInterfaceTargets Targets { get; }
    public string? Filter { get; set; }
    public string? Language { get; set; }
    public string? Body { get; set; }
    public string? FileName { get; set; }
}