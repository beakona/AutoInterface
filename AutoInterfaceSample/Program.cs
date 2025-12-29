#nullable enable
using BeaKona;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace AutoInterfaceSample.Test
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_TestDb.Info);
        }
    }

    interface IA
    {
        int X();
        int Y { get; }
    }

    interface IB : IA
    {
        Point Point { get; }
        int IA.X() => Point.X;
        int IA.Y => Point.Y;
    }


    partial class C1 : IB
    {

        [AutoInterface(typeof(IB))] private IB _inner = default!;

    }
    partial class D1 : IB
    {

        [AutoInterface(typeof(IB), IncludeBaseInterfaces = true)] private IB _inner = default!;

    }

}
 