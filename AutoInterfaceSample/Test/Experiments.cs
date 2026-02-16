using BeaKona;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace AutoInterfaceSample.Test;

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
    static int Count => 1;
    int Count2 => 2;
}

partial class C1 : IB
{
    [AutoInterface(typeof(IB), AllowMissingMembers = true)] private IB _inner = default!;

    System.Drawing.Point IB.Point => Point.Empty;
}

partial class C2 : IB
{
    [AutoInterface(typeof(IB))] private IB _inner = default!;

}

partial class D1 : IB
{

    [AutoInterface(typeof(IB), IncludeBaseInterfaces = true)] private IB _inner = default!;

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
}

//partial record TecProgDbConnection
//{
//[AllowNull]
//[DisallowNull]
//string IDb.ConnectionString
//{
//    get => (this.Inner as System.Data.IDbConnection)!.ConnectionString;
//    //[param:MaybeNull]
//    set => (this.Inner as System.Data.IDbConnection)!.ConnectionString = value;
//}
//}

public interface IDb
{
    [AllowNull]
    string ConnectionString
    {
        get;
        [param: AllowNull]
        set;
    }

    [AllowNull]
    string this[int a, [AllowNull] string b]
    {
        get;
        [param: AllowNull]
        set;
    }
}
