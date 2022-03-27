using System;

namespace BeaKona;

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