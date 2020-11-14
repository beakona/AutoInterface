using System;

namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            IPrintable person = new Person();
            person.Print();
            //Console.WriteLine(BeaKona.Output.Debug_Person.Info);
        }
    }

    public interface IPrintable
    {
        void Print();
    }

    public partial class Person : IPrintable
    {
        [BeaKona.AutoInterface(typeof(IPrintable))]
        private readonly IPrintable aspect1 = new PersonPrinterV1();
    }

    internal class PersonPrinterV1 : IPrintable
    {
        public void Print()
        {
            Console.WriteLine("Print");
        }
    }
}