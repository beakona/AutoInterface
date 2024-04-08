using System.Diagnostics.CodeAnalysis;

namespace AutoInterfaceSampleNetStandard
{
    partial class TestDb
    {
        [property: BeaKona.AutoInterface(typeof(IDb), IncludeBaseInterfaces = true)]
        public IDb Inner { get; set; } = default!;
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

    //partial class TestRecord
    //{
    //    [BeaKona.AutoInterface(IncludeBaseInterfaces = true)]
    //    public ITestable2? Testable { get; set; }
    //}
}

namespace System.Diagnostics.CodeAnalysis
{

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue)
        {
            this.ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute
    {
        public AllowNullAttribute()
        {
        }
    }
}
