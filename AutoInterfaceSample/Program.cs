using TestInterfaces.A.B;

namespace X.C
{
    namespace D
    {
        namespace E.F
        {
            namespace G
            {
                public interface IPrintable2
                {
                    void Print3();
                }
            }
        }
    }
}

namespace AutoInterfaceSample.Test.TT
{
    namespace T2
    {
        public interface ITest2
        {
            void Play();
        }
    }

    namespace T3
    {
        public interface ITest2
        {
            void Play();
        }

        namespace T4.T5
        {
            public class Program
            {
                public static void Main()
                {
                    //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person.Info);
                    //IPrintable<int> p = new Person();
                    //p.Print1();
                }
            }

            public class PrinterV1 : IPrintable<int>, X.C.D.E.F.G.IPrintable2, T2.ITest2
            {
                public int Length => 100;
                public int Count => 200;
                public void Print1()
                {
                }
                public void Print2()
                {
                }
                public void Print3()
                {
                }
                public void PrintTest()
                {
                }
                public void Play()
                {
                }
            }

            public partial class Person //: IPrintable//, IPrintable2
            {
                private void LogDebug(string name)
                {
                }

                //[BeaKona.AutoInterface]
                [BeaKona.AutoInterface(typeof(ITestable))]
                //[BeaKona.AutoInterface(typeof(ITestable))]
                //[BeaKona.AutoInterface(typeof(IPrintable), true)]
                //[BeaKona.AutoInterface(typeof(IPrintable), false)]
                //[BeaKona.AutoInterface(typeof(IPrintable))]//, TemplateBody = "void TestB1() {}"
                //[BeaKona.AutoInterface(typeof(IPrintable2))]//, TemplateBody = "void TestB2() {}"
                //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.PropertyGetter, Filter = "Length", Language = "scriban", Body = "return 1;")]
                //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print(\\d)?", Body = "LogDebug(nameof({{interface}}.{{name}})); {{expression}};")]
                private readonly IPrintable<int>? aspect1 = new PrinterV1();

                [BeaKona.AutoInterface(typeof(IPrintable<int>), IncludeBaseInterfaces = false)]
                [BeaKona.AutoInterface(typeof(X.C.D.E.F.G.IPrintable2))]
                //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
                //[BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
                //[BeaKona.AutoInterface(typeof(ITestable))]
                private readonly PrinterV1? aspect2 = new PrinterV1();

                [BeaKona.AutoInterface(typeof(T2.ITest2))]
                private readonly T2.ITest2? aspect3 = new PrinterV1();
            }
        }
    }
}
