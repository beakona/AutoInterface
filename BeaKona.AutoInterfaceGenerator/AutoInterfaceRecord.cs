using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class AutoInterfaceRecord : IMemberInfo
    {
        public AutoInterfaceRecord(ISymbol member, ITypeSymbol receiverType, AttributeData attribute, INamedTypeSymbol interfaceType, TemplateSettings? template)
        {
            this.Member = member;
            this.ReceiverType = receiverType;
            this.Attribute = attribute;
            this.InterfaceType = interfaceType;
            this.Template = template;
        }

        public ISymbol Member { get; }
        public ITypeSymbol ReceiverType { get; }
        public AttributeData Attribute { get; }
        public INamedTypeSymbol InterfaceType { get; }
        public TemplateSettings? Template { get; }
        public bool CastRequired => this.InterfaceType.Equals(this.ReceiverType, SymbolEqualityComparer.Default) == false;
    }
}