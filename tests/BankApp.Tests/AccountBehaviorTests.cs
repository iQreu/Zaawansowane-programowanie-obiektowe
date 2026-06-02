using BankApp.Core;
using BankApp.Core.Accounts;
using Xunit;

namespace BankApp.Tests;

public class AccountBehaviorTests
{
    [Fact]
    public void Checking_AllowsOverdraftUpToLimit()
    {
        var acc = new CheckingAccount("PL1", "PLN") { OverdraftLimit = 100m };
        acc.Deposit(new Money(50m));
        Assert.True(acc.CanWithdraw(new Money(150m)));
        Assert.False(acc.CanWithdraw(new Money(151m)));
    }

    [Fact]
    public void Savings_DoesNotAllowOverdraft()
    {
        var acc = new SavingsAccount("PL2", "PLN");
        acc.Deposit(new Money(50m));
        Assert.False(acc.CanWithdraw(new Money(51m)));
    }

    [Fact]
    public void Savings_CalculatesInterest()
    {
        var acc = new SavingsAccount("PL2", "PLN") { AnnualInterestRate = 0.12m };
        acc.Deposit(new Money(1000m));
        Assert.Equal(new Money(10m), acc.CalculateInterest());
    }

    [Fact]
    public void Credit_AllowsSpendingUpToCreditLimit()
    {
        var acc = new CreditAccount("PL3", "PLN") { CreditLimit = 500m };
        Assert.True(acc.CanWithdraw(new Money(500m)));
        Assert.False(acc.CanWithdraw(new Money(501m)));
    }

    [Fact]
    public void Polymorphism_MonthlyFeeDiffersByType()
    {
        Account checking = new CheckingAccount("PL1", "PLN");
        Account savings  = new SavingsAccount("PL2", "PLN");
        Assert.NotEqual(checking.CalculateMonthlyFee(), savings.CalculateMonthlyFee());
    }

    [Fact]
    public void Withdraw_BeyondLimit_ThrowsInsufficientFunds()
    {
        var acc = new SavingsAccount("PL2", "PLN");
        acc.Deposit(new Money(10m));
        Assert.Throws<InsufficientFundsException>(() => acc.Withdraw(new Money(20m)));
    }
}
