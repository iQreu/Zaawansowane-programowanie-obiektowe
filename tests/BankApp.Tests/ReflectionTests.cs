using System.Linq;
using BankApp.Core.Accounts;
using BankApp.Core.Reflection;
using Xunit;

namespace BankApp.Tests;

public class ReflectionTests
{
    [Fact]
    public void Registry_DiscoversAllAccountTypesViaReflection()
    {
        var types = AccountTypeRegistry.Discover().ToList();
        Assert.Contains(types, t => t.Key == "Checking");
        Assert.Contains(types, t => t.Key == "Savings");
        Assert.Contains(types, t => t.Key == "Credit");
        Assert.Equal(3, types.Count);
    }

    [Fact]
    public void Registry_ReadsDisplayNameFromAttribute()
    {
        var savings = AccountTypeRegistry.Discover().Single(t => t.Key == "Savings");
        Assert.Equal("Konto oszczędnościowe", savings.DisplayName);
    }

    [Fact]
    public void ReportGenerator_BuildsHeaderFromProperties()
    {
        var report = ReportGenerator.Generate(new[]
        {
            new SavingsAccount("PL1", "PLN")
        });
        Assert.Contains("Numer konta", report);
    }

    [Fact]
    public void ReportGenerator_UsesReportColumnAttributeHeader()
    {
        var report = ReportGenerator.Generate(new[] { new SavingsAccount("PL1", "PLN") });
        var headerLine = report.Split('\n')[0];
        Assert.Contains("Numer konta", headerLine);   // nagłówek z [ReportColumn]
        Assert.DoesNotContain("Number", headerLine);   // surowa nazwa właściwości nie powinna już być nagłówkiem
    }
}
