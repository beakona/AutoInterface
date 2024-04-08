#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace AutoInterfaceSample.Test
{
    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_TestRecord.Info);
        }
    }

    public class MyDb : IDb
    {
        public string ConnectionString { get; [param: AllowNull] set; } = default!;

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
        string ConnectionString
        {
            get;
            [param: AllowNull]
            set;
        }

        string this[int a, [AllowNull] string b]
        {
            get;
            [param: AllowNull]
            set;
        }
    }
}

/*namespace System.Diagnostics.CodeAnalysis
{

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }
}*/
