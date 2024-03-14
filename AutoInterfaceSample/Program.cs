using System;
using System.Diagnostics.CodeAnalysis;

namespace AutoInterfaceSample.Test
{
    public partial record TestRecord([property: BeaKona.AutoInterface] ILogger Logger)
    {
        //[field: BeaKona.AutoInterface]
        //public ILogger Logger { get; }

        //[BeaKona.AutoInterface]
        //private readonly ILogger logger2 = new SimpleLogger();
    }

    public sealed class LogEventProperty<T>
    {
    }

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.GenericParameter, AllowMultiple = true)]
    public class TestAttribute : Attribute
    {
    }

    public sealed class SimpleLogger : ILogger
    {
        public bool BindProperty<[Test] T>(string? propertyName, object? value, bool destructureObjects, [NotNullWhen(true)] out LogEventProperty<T>? property, [Test] params int[] values)
        {
            property = default;
            return false;
        }

        [Test]
        public int Count
        {
            [return: Test]
            [Test]
            get => 1;
            [return: Test]
            [Test]
            set
            {
            }
        }
        public int Length
        {
            get => 1;
            set { }
        }
    }

    public class Program
    {
        public static void Main()
        {
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_TestClass_1.Info);

            ILogger p = new TestRecord(new SimpleLogger());
            var result = p.BindProperty<int>("test", 1, false, out var property, 1, 2, 3);
        }
    }

    public interface ILogger
    {
        [Test]
        [return: Test]
        [return: Test]
        bool BindProperty<[Test] T>(string? propertyName, object? value, bool destructureObjects, [NotNullWhen(true)] out LogEventProperty<T>? property, [Test] params int[] values);

        [Test]
        int Count
        {
            [return: Test]
            [Test]
            get;
            [return: Test]
            [Test]
            set;
        }

        int Length
        {
            [return: Test]
            get;
            [return: Test]
            set;
        }
    }
}
