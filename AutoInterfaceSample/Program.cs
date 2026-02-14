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

    public interface IB
    {
        protected Point Point { get; }
        int X() => Point.X;
        int Y => Point.Y;
        static int Count => 1;
        int Count2 => 2;
    }

    partial class C1 : IB
    {
        [AutoInterface(typeof(IB), AllowMissingMembers = true)] private IB _inner = default!;

        System.Drawing.Point IB.Point => Point.Empty;
    }

    public class MyDb : IDb
    {
        [AllowNull]
        public string ConnectionString { get; [param: AllowNull] set; } = default!;

        [AllowNull]
        public string this[int a, [AllowNull] string b]
        {
            get => b ?? "";
            [param: AllowNull]
            set
            {
            }
        }
    }

    partial record TestDb([property: BeaKona.AutoInterface(typeof(IDb), IncludeBaseInterfaces = true)] IDb Inner) //: IDb
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
 