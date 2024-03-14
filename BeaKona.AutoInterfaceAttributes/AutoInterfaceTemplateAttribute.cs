using System;
using System.Diagnostics;

namespace BeaKona;

[Conditional("CodeGeneration")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class AutoInterfaceTemplateAttribute(AutoInterfaceTargets targets) : Attribute
{
    public AutoInterfaceTargets Targets { get; } = targets;
    public string? Filter { get; set; }
    public string? Language { get; set; }
    public string? Body { get; set; }
    public string? FileName { get; set; }
}
