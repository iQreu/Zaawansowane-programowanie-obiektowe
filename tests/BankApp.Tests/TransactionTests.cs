using BankApp.Core;
using Xunit;

namespace BankApp.Tests;

public class TransactionTests
{
    [Fact]
    public void Withdrawal_HasNegativeSignedAmount()
    {
        var t = new Transaction(TransactionType.Withdrawal, new Money(50m), "test");
        Assert.Equal(-50m, t.SignedAmount);
    }

    [Fact]
    public void Deposit_HasPositiveSignedAmount()
    {
        var t = new Transaction(TransactionType.Deposit, new Money(50m), "test");
        Assert.Equal(50m, t.SignedAmount);
    }
}
