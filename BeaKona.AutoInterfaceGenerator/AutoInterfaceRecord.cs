using Microsoft.CodeAnalysis;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class AutoInterfaceRecord : IMemberInfo
    {
        public AutoInterfaceRecord(ISymbol member, ITypeSymbol receiverType, AttributeData attribute, INamedTypeSymbol interfaceType, bool allowNullConditionOperator)
        {
            this.Member = member;
            this.ReceiverType = receiverType;
            this.Attribute = attribute;
            this.InterfaceType = interfaceType;
            this.AllowNullConditionOperator = allowNullConditionOperator;
        }

        public ISymbol Member { get; }
        public ITypeSymbol ReceiverType { get; }
        public AttributeData Attribute { get; }
        public INamedTypeSymbol InterfaceType { get; }
        public bool AllowNullConditionOperator { get; }
        public bool CastRequired => this.InterfaceType.Equals(this.ReceiverType, SymbolEqualityComparer.Default) == false;
    }
}