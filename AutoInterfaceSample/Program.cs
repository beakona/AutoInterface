using TestInterfaces.A.B;

namespace AutoInterfaceSample.Test
{
    public partial record TestRecord([property: BeaKona.AutoInterface(IncludeBaseInterfaces = true)] ITestable Testable)
    {
    }

    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_TestRecord.Info);

            ITestable p = new TestRecord(new SimpleLogger());

            p.Test();

            //p.Log<int?>(LogLevel.Debug, default, 1, null, null);
            //var result = p.BindProperty<int>("test", 1, false, out var property, 1, 2, 3);
        }
    }
}
