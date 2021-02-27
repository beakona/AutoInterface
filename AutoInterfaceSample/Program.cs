using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoInterfaceSample
{
    public class Program
    {
        //async mora imati ! ako je nullable
        //getter kojem je return nullable moze imati? a ako nije nullable onda mora imati !
        //setter kojem je tip nullable mora imati ! i nikad ?
        //event kao i setter

        public static void Main()
        {
            //IPrintable<int> person = new Person<int>();
            //int b = 5;
            //person.@object(out _, ref b, null, null);
            //System.Diagnostics.Debug.WriteLine(BeaKona.Output.Debug_Person_1.Info);
        }
    }

    public interface IPrintable<G>
    {
        void @object(out int a, ref int b, in TimeSpan? c, string? d, int @object = 5);
        int Print2<T>(params T[] other);
        int? Print3();
        string Print4<T>(params T[] other);
        string? Print5();
        Task<int> Print1Async<T>(CancellationToken cancel = default);
        Task<int?> Print2Async<T>(CancellationToken cancel = default);
        Task<object> Print3Async<T>(CancellationToken cancel = default);
        Task<object?> Print4Async<T>(CancellationToken cancel = default);

        int Count { get; }
        TimeSpan? Timeout { get; set; }
        TimeSpan Timeout2 { get; set; }
        object? this[int count, in int a, params int[] other] { get; set; }
        object this[in int a, params int[] other] { get; set; }
        int? this[double count, in int a, params int[] other] { get; set; }
        int this[in double a, params int[] other] { get; set; }

        event EventHandler Close;
        event EventHandler? Close2;
    }

    public interface IPrintableEx<G> : IPrintable<G>
    {
    }

    //internal class PersonPrinterV1 : IPrintable
    //{
    //    public void Print1()
    //    {
    //        Console.WriteLine("Print1");
    //    }
    //    public void Print2()
    //    {
    //        Console.WriteLine("Print2");
    //    }
    //}

    public partial class Person<T> : IPrintable<int>
    {
        //[BeaKona.AutoInterface(TemplateLanguage = "scriban", TemplateBody = TemplateBody)]
        //private readonly IPrintable<int>? aspect1 = null; //new PersonPrinterV1();

        [BeaKona.AutoInterface(typeof(IPrintable<int>))]
        private readonly IPrintableEx<int>? aspect1;

        [BeaKona.AutoInterface(typeof(IPrintable<int>))]
        private readonly IPrintableEx<int>? aspect2;

        [BeaKona.AutoInterface(typeof(IPrintable<int>))]
        private readonly IPrintable<int> aspect3;

        [BeaKona.AutoInterface(typeof(IPrintable<int>))]
        private readonly IPrintableEx<int> aspect4;

        [BeaKona.AutoInterface(typeof(IPrintable<int>))]
        private readonly IPrintableEx<int>? unused;

        const string TemplateBody = @"
{{~for method in methods~}}
{{method.is_async?""async "":""""}}{{method.return_type}} {{interface}}.{{method.name}}({{method.arguments_definition}})
{
{{~if method.is_async~}}
    {{~for reference in references~}}
    var temp{{for.index}} = (({{interface}})this.{{reference}}).{{method.name}}({{method.call_arguments}}).ConfigureAwait(false);
    {{~end~}}
    {{~for reference in references~}}
    {{for.last && method.return_expected ? ""return "" : """"}}await temp{{for.index}};
    {{~end~}}
{{~else~}}
    {{~for reference in references~}}
    {{for.last && method.return_expected ? ""return "" : """"}}(({{interface}})this.{{reference}}).{{method.name}}({{method.call_arguments}});
    {{~end~}}
{{~end~}}
}

{{~end~}}
{{~for property in properties~}}
{{property.type}} {{interface}}.{{property.name}}
{
{{~if property.have_getter~}}
   get
   {
      return (({{interface}})this.{{references[0]}}).{{property.name}};
   }
{{~end~}}
{{~if property.have_setter~}}
   set
   {
      {{~for reference in references~}}
      (({{interface}})this.{{reference}}).{{property.name}} = value;
      {{~end~}}
   }
{{~end~}}
}

{{~end~}}
{{~for indexer in indexers~}}
{{indexer.type}} {{interface}}.{{indexer.name}}[{{indexer.parameters_definition}}]
{
{{~if indexer.have_getter~}}
   get
   {
      return (({{interface}})this.{{references[0]}})[{{indexer.call_parameters}}];
   }
{{~end~}}
{{~if indexer.have_setter~}}
   set
   {
      {{~for reference in references~}}
      (({{interface}})this.{{reference}})[{{indexer.call_parameters}}] = value;
      {{~end~}}
   }
{{~end~}}
}

{{~end~}}
{{~for event in events~}}
event {{event.type}} {{interface}}.{{event.name}}
{
   add
   {
      {{~for reference in references~}}
      (({{interface}})this.{{reference}}).{{event.name}} += value;
      {{~end~}}
   }
   remove
   {
      {{~for reference in references~}}
      (({{interface}})this.{{reference}}).{{event.name}} -= value;
      {{~end~}}
   }
}

{{~end~}}
";
    }
}
