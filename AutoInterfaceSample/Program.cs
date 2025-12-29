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

    interface IB
    {
        protected Point Point { get; }
        int X() => Point.X;
        int Y => Point.Y;
    }


    partial class C1 : IB
    {

        [AutoInterface(typeof(IB), AllowMissingMembers = true)] private IB _inner = default!;

    }


}
 