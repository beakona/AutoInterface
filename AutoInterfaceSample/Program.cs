namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person_1.Info);
            IPrintable p = new Proxy();
            p.Print();
        }
    }

    public interface IPrintable
    {
        void Print();
    }

    public class Printer : IPrintable
    {
        public void Print()
        {
        }
    }

    public partial class Proxy : IPrintable
    {
        [BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method, Filter = "Print2", Body = "/* */")]
        private IPrintable? aspect1 = new Printer();
    }
}
