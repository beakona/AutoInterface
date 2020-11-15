# AutoInterface

C# [Source Generator](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) which redirects all interface-calls to a backing member.
<br>

## How to use it?

Inside your .NET project:
1. Install NuGet package `BeaKona.AutoInterfaceGenerator` or add `BeaKona.AutoInterfaceGenerator.dll` as Analyzer to your Visual Studio `.csproj` file.
```xml
<ItemGroup>
   <Analyzer Include="absolute-or-relative-path-to\BeaKona.AutoInterfaceGenerator.dll"/>
</ItemGroup>
```
2. Mark class as `partial`.
3. Explicitly enlist target interface in class interface list [NOTE: this is a design decision, not technical restriction].
4. Append attribute `BeaKona.AutoInterfaceAttribute` to a backing member which will handle calls.
<br><br>

## Simple scenario - inferred interface type

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

## Explicitly defined interface type

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface(typeof(IPrintable))]
   private readonly PersonPrinterV1 aspect1 = new PersonPrinterV1();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print() => (this.aspect1 as IPrintable)?.Print();
}
```
<br>

## Single interface - multiple backers

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface]
   private readonly IPrintable aspect1 = new PersonPrinterPart1();

   [BeaKona.AutoInterface]
   private readonly IPrintable aspect2 = new PersonPrinterPart2();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print()
   {
      this.aspect1.Print();
      this.aspect2.Print();
   }
}
```
<br>

## Multiple interfaces

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

interface IScannable
{
   void Scan();
}

public partial class Person : IPrintable, IScannable
{
   [BeaKona.AutoInterface(typeof(IPrintable))]
   [BeaKona.AutoInterface(typeof(IScannable))]
   private readonly Printer aspect1 = new Printer();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print() => (this.aspect1 as IPrintable)?.Print();
   void IScannable.Scan() => (this.aspect1 as IScannable)?.Scan();
}
```
<br>

## Custom member implementation

Manually written source:

```csharp
public interface IPrintable
{
   void Print();
   void Align();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface]
   private readonly IPrintable aspect1 = new PersonPrinterV1();

   void IPrintable.Print()
   {
      //custom implementation which will prevent auto-generation
   }
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Align() => this.aspect1.Align();
}
```
<br>

## Methods, properties, indexers, events

Manually written source:

```csharp
interface ITest
{
   int Age { get; }
   int this[in int a, int b = 5] { get; set; }

   event EventHandler<EventArgs> Done;

   void Method1(int a, out int b, ref int c, in int d, params int[] e);
   void Method2(int a = 1);
}

public partial class Person : ITest
{
   [BeaKona.AutoInterface]
   private readonly ITest aspect1 = new PersonTest();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   int ITest.Age => this.aspect1.Age;

   int ITest.this[in int a, int b]
   {
      get => this.aspect1[in a, b];
      set => this.aspect1[in a, b] = value;
   }

   event System.EventHandler<System.EventArgs> ITest.Done
   {
      add => this.aspect1.Done += value;
      remove => this.aspect1.Done -= value;
   }

   void ITest.Method1(int a, out int b, ref int c, in int d, params int[] e) => this.aspect1.Method1(a, out b, ref c, in d, e);

   void ITest.Method2(int a = 1) => this.aspect1.Method2(a);
}
```
<br>

## Class, record, struct

Manually written source:

```csharp
public interface IPrintable
{
   void Print();
}

public partial record PersonR : IPrintable
{
   [BeaKona.AutoInterface]
   private readonly IPrintable aspect1 = new RecordPrinter();
}

public partial class PersonC : IPrintable
{
   [BeaKona.AutoInterface]
   private readonly IPrintable aspect1 = new ClassPrinter();
}

public partial struct PersonS : IPrintable
{
   public PersonS(IPrintable aspect1) => this.aspect1 = aspect1;

   [BeaKona.AutoInterface]
   private readonly IPrintable aspect1;
}
```

Auto-generated accompanying source:

```csharp
partial record PersonR
{
   void IPrintable.Print() => this.aspect1.Print();
}
partial class PersonC
{
   void IPrintable.Print() => this.aspect1.Print();
}
partial struct PersonS
{
   void IPrintable.Print() => this.aspect1.Print();
}
```
<br>

## Null condition operator (?., ?[])

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface(AllowNullConditionOperator = true)]
   private readonly IPrintable? aspect1 = null;
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print() => this.aspect1?.Print();
}
```
<br>

## Async

Manually written source:


```csharp
interface ITest
{
   Task Method1Async();
   Task<int> Method2Async();

   ValueTask Method3Async();
   ValueTask<int> Method4Async();
}

public partial class Person : ITest
{
   [BeaKona.AutoInterface]
   private readonly ITest aspect1 = new Test();

   [BeaKona.AutoInterface]
   private readonly ITest aspect2 = new Test();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   async Task ITest.Method1Async()
   {
      await this.aspect1.Method1Async();
      await this.aspect2.Method1Async();
   }

   async Task<int> ITest.Method2Async()
   {
      await this.aspect1.Method2Async();
      return await this.aspect2.Method2Async();
   }

   async ValueTask ITest.Method3Async()
   {
     await this.aspect1.Method3Async();
      await this.aspect2.Method3Async();
   }

   async ValueTask<int> ITest.Method4Async()
   {
      await this.aspect1.Method4Async();
      return await this.aspect2.Method4Async();
   }
}
```
<br>

## Value tuples

Manually written source:

```csharp
interface IPrintable
{
   void Print((int x, int y) position);
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
   void IPrintable.Print((int x, int y) position) => this.aspect1.Print(position);
}
```
<br>

## Type casting

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

interface IPrintableEx : IPrintable
{
   void PrintEx();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface(typeof(IPrintable))]
   private readonly IPrintableEx aspect1 = new PersonPrinterV1();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print() => ((IPrintable)this.aspect1).Print();
}
```
<br>

## Conflicting type argument name

Manually written source:

```csharp
interface ITest
{
   T F<T>(IEnumerable<T> elements);
}

public partial class Person<T> : ITest
{
   [BeaKona.AutoInterface]
   private readonly ITest aspect1 = new Test();
}
```

Auto-generated accompanying source:

```csharp
partial class Person<T>
{
   T1 ITest.F<T1>(IEnumerable<T1> elements) => this.aspect1.F<T1>(elements);
}
```
<br>

## Assembly alias

Manually written source:

```csharp
extern alias alias1;

public partial class Person : alias1::Some.IPrintable
{
   [BeaKona.AutoInterface]
   private readonly alias1::Some.IPrintable aspect1 = new PersonPrinterV1();
}
```

Auto-generated accompanying source:

```csharp
extern alias alias1;

partial class Person
{
   void alias1::Some.IPrintable.Print() => this.aspect1.Print();
}
```
<br>

## Nested types

Manually written source:

```csharp
namespace Scope
{
   public class Wrapper
   {
      public interface IPrintable
      {
         void Print();
      }
   }
}

public partial class Person : Scope.Wrapper.IPrintable
{
   [BeaKona.AutoInterface]
   private readonly Scope.Wrapper.IPrintable aspect1 = new PersonPrinterV1();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void Scope.Wrapper.IPrintable.Print() => this.aspect1.Print();
}
```
<br>

## Verbatim names

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

public partial class Person : IPrintable
{
   [BeaKona.AutoInterface]
   private readonly IPrintable @object = new PersonPrinterV1();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   void IPrintable.Print() => this.@object.Print();
}
```
<br>

## Error codes

*BK-AG* stands for BeaKona-AutoGenerator.


|  Code   |              Description [en-US]              |
|:-------:|:----------------------------------------------|
| BK-AG01 | Type is not marked as partial. |
| BK-AG02 | Type is static. |
| BK-AG03 | Type is not an interface. |
| BK-AG04 | Unable to cast. |
| BK-AG05 | Type does not implement defined interface(s). |
| BK-AG06 | Target member is writeonly. |
| BK-AG07 | Type should be an interface. |
| BK-AG08 | Type is not a class, record or struct. |
| BK-AG09 | Internal exception. |

<br>

## Additional links

[Introducing C# source generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/)  
[Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.cookbook.md)

---

![.NET Core](https://github.com/beakona/AutoInterface/workflows/.NET%20Core/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/BeaKona.AutoInterfaceGenerator)](https://www.nuget.org/packages/BeaKona.AutoInterfaceGenerator)
