#nullable enable
namespace BeaKona
{
    using System;

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
        public bool IncludeBaseInterfaces { get; set; }
        public string? TemplateLanguage { get; set; }
        public string? TemplateBody { get; set; }
        public string? TemplateFileName { get; set; }
    }

    [Flags]
    public enum AutoInterfaceTargets : byte
    {
        Method = 0x01,
        PropertyGetter = 0x02,
        PropertySetter = 0x04,
        IndexerGetter = 0x08,
        IndexerSetter = 0x10,
        EventAdder = 0x20,
        EventRemover = 0x40,
        Property = PropertyGetter | PropertySetter,
        Indexer = IndexerGetter | IndexerSetter,
        Event = EventAdder | EventRemover,
        Getter = PropertyGetter | IndexerGetter,
        Setter = PropertySetter | IndexerSetter,
        All = Method | Property | Indexer | Event,
    }

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
}
