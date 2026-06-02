using BankApp.Core;
using Xunit;

namespace BankApp.Tests;

public class MoneyTests
{
    [Fact]
    public void Addition_AddsAmounts()
    {
        var sum = new Money(10m) + new Money(5m);
        Assert.Equal(new Money(15m), sum);
    }

    [Fact]
    public void Subtraction_SubtractsAmounts()
    {
        Assert.Equal(new Money(5m), new Money(10m) - new Money(5m));
    }

    [Fact]
    public void Multiplication_ByDecimal_Scales()
    {
        Assert.Equal(new Money(20m), new Money(10m) * 2m);
    }

    [Fact]
    public void Comparison_Operators_Work()
    {
        Assert.True(new Money(10m) > new Money(5m));
        Assert.True(new Money(5m) < new Money(10m));
        Assert.True(new Money(10m) >= new Money(10m));
        Assert.True(new Money(10m) <= new Money(10m));
    }

    [Fact]
    public void Equality_ConsidersAmountAndCurrency()
    {
        Assert.True(new Money(10m, "PLN") == new Money(10m, "PLN"));
        Assert.True(new Money(10m, "PLN") != new Money(10m, "EUR"));
    }

    [Fact]
    public void Operation_OnDifferentCurrencies_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new Money(1m, "PLN") + new Money(1m, "EUR"));
    }

    [Fact]
    public void ExplicitCast_ToDecimal_ReturnsAmount()
    {
        Assert.Equal(10m, (decimal)new Money(10m));
    }
}
