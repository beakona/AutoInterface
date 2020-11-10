using System;
using System.Collections.Generic;

namespace AutoInterfaceSample
{
    public class Program
    {
        public static void Main()
        {
            //Console.WriteLine(BeaKona.Output.Debug_Person.Info);
        }
    }

    public interface IPrintable
    {
        void Print(int a, int? b, string c, string? d, int[] e, int[]? f, IEnumerable<int?>? g);
    }

    public partial class Person : IPrintable
    {
        [BeaKona.AutoInterface(typeof(IPrintable))]
        private readonly IPrintable aspect1 = new PersonPrinterV1();

        //public void Print(int a, int? b, string c, string? d, int[] e, int[]? f, IEnumerable<int?>? g)
        //{
        //}
    }

    internal class PersonPrinterV1 : IPrintable
    {
        public void Print(int a, int? b, string c, string? d, int[] e, int[]? f, IEnumerable<int?>? g)
        {
            Console.WriteLine("Print");
        }
    }
}