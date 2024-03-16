using TestInterfacesNetStandard;

namespace AutoInterfaceSample.Test
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_TestRecord.Info);
        }
    }

    partial record TestRecord
    {
        //[BeaKona.AutoInterface(IncludeBaseInterfaces = true)]
        public ITestable2? Testable { get; set; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }
}
