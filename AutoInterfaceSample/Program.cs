using TestInterfaces.A.B;

namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person.Info);
            IArbitrary p = new Person();
            p.Method2();
        }
    }

    public interface IArbitrary
    {
        void Method1();
        void Method2();
    }

    public class PrinterV1 : IPrintable
    {
        public void Print()
        {
        }

        public void Method1()
        {
        }

        public void Method2()
        {
        }
    }

    public partial class Person
    {
        [BeaKona.AutoInterface(typeof(IArbitrary))]
        //[BeaKona.AutoInterface(typeof(ITestable))]
        //[BeaKona.AutoInterface(typeof(IPrintable), true)]
        //[BeaKona.AutoInterface(typeof(IPrintable), false)]
        //[BeaKona.AutoInterface(typeof(IPrintable))]//, TemplateBody = "void TestB1() {}"
        //[BeaKona.AutoInterface(typeof(IPrintable2))]//, TemplateBody = "void TestB2() {}"
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.PropertyGetter, Filter = "Length", Language = "scriban", Body = "return 1;")]
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print(\\d)?", Body = "LogDebug(nameof({{interface}}.{{name}})); {{expression}};")]
        private readonly PrinterV1? aspect1 = new PrinterV1();

        //[BeaKona.AutoInterface(typeof(IPrintable), IncludeBaseInterfaces = true)]
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
        //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
        //[BeaKona.AutoInterface(typeof(ITestable))]
        //private readonly PrinterV1? aspect2 = new PrinterV1();
    }
}
