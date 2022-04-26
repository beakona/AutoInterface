using System;

namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person.Info);
            IArbitrary<int> p = new Person();
            int f;
            int g = 0;
            p.Method(1, out f, ref g, "t", 1, 2, 3);
        }
    }


    public class SignalPlotXYConst<TX, TY, T> where TX : struct, IComparable
    {
    }

    public interface ISome
    {
    }

    public interface ISome2
    {
    }

    public interface IArbitrary<T>
    {
        T Length { get; }
        int? Method(int a, out int b, ref int c, dynamic d, params int[] p);

        SignalPlotXYConst<TXi, TYi?, T>? Method2<TXi, TYi>(TXi x, TYi[] ys, in int s) where TXi : struct, IComparable where TYi : struct, IComparable, ISome, ISome2;
    }

    public class PrinterV1
    {
        public void Print()
        {
        }

        public int Length => 1;

        public int? Method(int a, out int b, ref int c, dynamic d, params int[] p)
        {
            b = a;
            return a;
        }

        public SignalPlotXYConst<TXc, TYc?, int>? Method2<TXc, TYc>(TXc x, TYc[] ys, in int s) where TXc : struct, IComparable where TYc : struct, IComparable, ISome, ISome2
        {
            return null;
        }
    }

    public partial class Person
    {
        [BeaKona.AutoInterface(typeof(IArbitrary<int>), PreferCoalesce = true)]
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
