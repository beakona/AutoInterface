using System.Collections.Generic;

namespace BeaKona.AutoInterfaceGenerator.Templates
{
    internal interface IRootModel
    {
        string? Interface { get; set; }

        List<string> References { get; }
        string? Reference { get; }
    }
}
