# AutoInterface

C# [Source Generator](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) which redirects all interface-calls to backing member.

Hand writtend source:

```csharp
interface IPrintable
{
  void Print();
}

public partial class Person
{
  [AutoInterface(typeof(IPrintable))]
  private readonly IPrintable aspect1 = new PersonPrinterV1();
}
```

Generated source:

```csharp
partial class Person : IPrintable
{
  void IPrintable.Print() => this.aspect1.Print();
}
```
