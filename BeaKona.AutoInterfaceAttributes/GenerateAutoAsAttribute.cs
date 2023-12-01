using System;
using System.Diagnostics;

namespace BeaKona;

[Conditional("CodeGeneration")]
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GenerateAutoAsAttribute : Attribute
{
    public bool EntireInterfaceHierarchy { get; set; } = false;
    public bool SkipSystemInterfaces { get; set; } = true;
}
