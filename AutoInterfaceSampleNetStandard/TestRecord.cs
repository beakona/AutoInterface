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

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute
    {
        public AllowNullAttribute()
        {
        }
    }
}
