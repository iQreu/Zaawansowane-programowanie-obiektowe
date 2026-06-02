using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Core.Services;
using BankApp.Tests.Fakes;
using Xunit;

namespace BankApp.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task Deposit_IncreasesBalance_AndRaisesEvent()
    {
        var acc = new SavingsAccount("PL1", "PLN");
        var repo = new FakeAccountRepository(acc);
        var service = new TransactionService(repo);

        TransactionEventArgs? captured = null;
        service.TransactionCompleted += (_, e) => captured = e;

        var result = await service.DepositAsync("PL1", new Money(100m));

        Assert.True(result.IsSuccess);
        Assert.Equal(new Money(100m), acc.Balance);
        Assert.NotNull(captured);
        Assert.Equal("PL1", captured!.AccountNumber);
    }

    [Fact]
    public async Task Withdraw_BeyondFunds_ReturnsFailure()
    {
        var acc = new SavingsAccount("PL1", "PLN");
        var repo = new FakeAccountRepository(acc);
        var service = new TransactionService(repo);

        var result = await service.WithdrawAsync("PL1", new Money(50m));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Transfer_MovesMoneyBetweenAccounts()
    {
        var from = new SavingsAccount("PL1", "PLN");
        var to   = new SavingsAccount("PL2", "PLN");
        from.Deposit(new Money(100m));
        var repo = new FakeAccountRepository(from, to);
        var service = new TransactionService(repo);

        var result = await service.TransferAsync("PL1", "PL2", new Money(40m));

        Assert.True(result.IsSuccess);
        Assert.Equal(new Money(60m), from.Balance);
        Assert.Equal(new Money(40m), to.Balance);
    }

    [Fact]
    public async Task Deposit_UnknownAccount_ReturnsFailure()
    {
        var service = new TransactionService(new FakeAccountRepository());
        var result = await service.DepositAsync("BRAK", new Money(10m));
        Assert.False(result.IsSuccess);
    }
}
