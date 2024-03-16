using TestInterfacesNetStandard;

namespace AutoInterfaceSampleNetStandard
{
    partial class TestRecord
    {
        [BeaKona.AutoInterface(IncludeBaseInterfaces = true)]
        public ITestable2? Testable { get; set; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue)
        {
            this.ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }
}
