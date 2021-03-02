# AutoInterface

C# [Source Generator](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) which redirects all interface-calls to one or more backing members. Source code can be generated for a `class`, `record`, or `struct`. It can be generated automatically or by a custom template ([scriban](https://github.com/scriban/scriban), liquid).
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

## Generate code from template

Manually written source:

```csharp
interface IPrintable
{
   void Print();
}

public partial class Person : IPrintable
{
   // add file mytemplate.scriban in yout VS project
   // and set it's build action to: 'C# analyzer additional file'
   [BeaKona.AutoInterface(TemplateFileName = "mytemplate.scriban")]
   private readonly IPrintable? aspect1 = null;
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   ..generated from file..
}
```
<br>

## Partial template

Partial template can be defined inline (from string) or from file.

Manually written source:

```csharp
interface IPrintable
{
   int Length { get; }
   int Count { get; }
   void Print1();
   void Print2();
}

public partial class Person : IPrintable
{
   private void LogDebug(string name) { }

   [BeaKona.AutoInterface]
   [BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.PropertyGetter,
        Filter = "Length", Language = "scriban", Body = "return 1;")]
   [BeaKona.AutoInterfaceTemplate(BeaKona.AutoInterfaceTargets.Method,
        Filter = "Print(\\d)?", Body="LogDebug(nameof({{interface}}.{{name}})); {{expression}};")]
   private readonly IPrintable? aspect1 = new PrinterV1();
}
```

Auto-generated accompanying source:

```csharp
partial class Person
{
   int IPrintable.Length
   {
      return 1;
   }
   
   int IPrintable.Count => this.aspect1!.Count;

   void IPrintable.Print1()
   {
       LogDebug(nameof(IPrintable.Print1));
       this.aspect1!.Print1();
   }

   void IPrintable.Print2()
   {
       LogDebug(nameof(IPrintable.Print2));
       this.aspect1!.Print2();
   }
}
```

<br>

Other examples can be found in [wiki](https://github.com/beakona/AutoInterface/wiki/Examples).

<br>
---

![.NET Core](https://github.com/beakona/AutoInterface/workflows/.NET%20Core/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/BeaKona.AutoInterfaceGenerator)](https://www.nuget.org/packages/BeaKona.AutoInterfaceGenerator)
