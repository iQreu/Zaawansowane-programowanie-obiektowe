namespace BankApp.Core.Attributes;

/// <summary>Oznacza właściwość jako kolumnę raportu (czytane przez refleksję w ReportGenerator).</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ReportColumnAttribute : Attribute
{
    public string Header { get; }
    public ReportColumnAttribute(string header) => Header = header;
}
