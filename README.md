# AutoInterface

C# [Source Generator](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) which redirects all interface-calls to a backing member. Source can be generated for `class`, `record`, and `struct`.
<br>

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface]
   private readonly IPrintable aspect1 = new PersonPrinterV1();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print() => this.aspect1.Print();
}
```
<br>

Other examples can be found in [wiki](https://github.com/beakona/AutoInterface/wiki/Examples).

<br>

---

![.NET Core](https://github.com/beakona/AutoInterface/workflows/.NET%20Core/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/BeaKona.AutoInterfaceGenerator)](https://www.nuget.org/packages/BeaKona.AutoInterfaceGenerator)
