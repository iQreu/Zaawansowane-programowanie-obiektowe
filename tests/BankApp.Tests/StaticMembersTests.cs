using BankApp.Core;
using BankApp.Core.Accounts;
using Xunit;

namespace BankApp.Tests;

public class StaticMembersTests
{
    [Fact]
    public void NumberGenerator_ProducesUniqueIncreasingNumbers()
    {
        var a = AccountNumberGenerator.Next();
        var b = AccountNumberGenerator.Next();
        Assert.NotEqual(a, b);
        Assert.StartsWith("PL", a);
    }

    [Fact]
    public void CurrencyFormatter_FormatsMoney()
    {
        var formatted = CurrencyFormatter.Format(new Money(1234.5m));
        Assert.EndsWith("PLN", formatted);
        Assert.Contains("1", formatted);
    }

    [Fact]
    public void Factory_CreatesRequestedType()
    {
        Assert.IsType<SavingsAccount>(AccountFactory.Create("Savings"));
        Assert.IsType<CreditAccount>(AccountFactory.Create("Credit"));
        Assert.IsType<CheckingAccount>(AccountFactory.Create("Checking"));
    }

    [Fact]
    public void Factory_UnknownType_Throws()
    {
        Assert.Throws<ArgumentException>(() => AccountFactory.Create("???"));
    }
}
