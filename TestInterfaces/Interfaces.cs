using System;

namespace TestInterfaces
{
    public interface ITestable
    {
        void PrintTest();
    }

    public interface IPrintable<T> : ITestable
    {
        int Length { get; }
        int Count { get; }
        void Print1();
        void Print2();
    }

    public interface IPrintable2
    {
        void Print3();
    }
}
