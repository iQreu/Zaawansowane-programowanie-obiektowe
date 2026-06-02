using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Core.Interest;
using Xunit;

namespace BankApp.Tests;

public class InterestStrategyTests
{
    [Fact]
    public void StandardStrategy_UsesAccountInterest()
    {
        var acc = new SavingsAccount("PL2", "PLN") { AnnualInterestRate = 0.12m };
        acc.Deposit(new Money(1000m));
        IInterestStrategy strategy = new StandardInterestStrategy();
        Assert.Equal(new Money(10m), strategy.Calculate(acc));
    }

    [Fact]
    public void PromotionalStrategy_AddsBonus()
    {
        var acc = new SavingsAccount("PL2", "PLN") { AnnualInterestRate = 0.12m };
        acc.Deposit(new Money(1000m));
        IInterestStrategy strategy = new PromotionalInterestStrategy(bonus: new Money(5m));
        Assert.Equal(new Money(15m), strategy.Calculate(acc));
    }

    [Fact]
    public void DelegateStrategy_UsesProvidedFunction()
    {
        var acc = new SavingsAccount("PL2", "PLN");
        acc.Deposit(new Money(100m));
        IInterestStrategy strategy = new DelegateInterestStrategy(a => new Money(1m, "PLN"));
        Assert.Equal(new Money(1m), strategy.Calculate(acc));
    }
}
