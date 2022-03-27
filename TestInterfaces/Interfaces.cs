namespace TestInterfaces.A.B
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
}
