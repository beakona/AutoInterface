using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class AutoInterfaceInfo
    {
        public AutoInterfaceInfo(ISymbol member, ITypeSymbol receiverType, AttributeData attribute, INamedTypeSymbol interfaceType)
        {
            this.Member = member;
            this.ReceiverType = receiverType;
            this.Attribute = attribute;
            this.InterfaceType = interfaceType;
        }

        public ISymbol Member { get; }
        public ITypeSymbol ReceiverType { get; }
        public AttributeData Attribute { get; }
        public INamedTypeSymbol InterfaceType { get; }
        public bool CastRequired => this.InterfaceType.Equals(this.ReceiverType, SymbolEqualityComparer.Default) == false;
    }
}