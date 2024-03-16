using System.Diagnostics.CodeAnalysis;

namespace TestInterfacesNetStandard
{
    public interface ITestable2
    {
        bool TryParse(string s, [NotNullWhen(true)] out int? value);
    }
}
