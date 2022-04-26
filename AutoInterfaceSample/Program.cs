using System.Runtime.InteropServices;
using TestInterfaces.A.B;

namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person.Info);
            IArbitrary p = new Person();
            int f;
            int? g = 0;
            p.Method2(1, out f, ref g, new PrinterV1(), null, 5);
        }
    }

    public interface IArbitrary
    {
        int Length { get; }
        void Method1();
        int Method2(int a, out int b, ref int? c, IPrintable e1, IPrintable? e2, in int f);
    }

    public class PrinterV1 : IPrintable
    {
        public void Print()
        {
        }

        public void Method1()
        {
        }

        public int Length => 1;

        public int Method2([In] int b, out int c, ref int? d, IPrintable e1, IPrintable? e2, in int f)
        {
            c = b;
            return b;
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
