namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person.Info);
            IPrintable p = new Person();
            p.Print1();
        }
    }
    public interface ITestable
    {
        void Test();
    }
    public interface IPrintable : ITestable
    {
        int Length { get; }
        int Count { get; }
        void Print1();
        void Print2();
    }

    public class PrinterV1 : IPrintable
    {
        public int Length => 100;
        public int Count => 200;
        public void Print1()
        {
        }
        public void Print2()
        {
        }
        public void Test()
        {

        }
    }

    public partial class Person : IPrintable
    {
        private void LogDebug(string name)
        {
        }

        [BeaKona.AutoInterface]
        [BeaKona.AutoInterface]
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.PropertyGetter, Filter = "Length", Language = "scriban", Body = "return 1;")]
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print(\\d)?", Body = "LogDebug(nameof({{interface}}.{{name}})); {{expression}};")]
        private readonly IPrintable? aspect1 = new PrinterV1();

        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
        [BeaKona.AutoInterface]
        [BeaKona.AutoInterface]
        private readonly IPrintable? aspect2 = new PrinterV1();
    }
}
