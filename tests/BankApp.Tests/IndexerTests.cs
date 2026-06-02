using BankApp.Core;
using BankApp.Core.Accounts;
using Xunit;

namespace BankApp.Tests;

public class IndexerTests
{
    [Fact]
    public void Customer_Indexer_ReturnsNthAccount()
    {
        var c = new Customer("Jan", "Kowalski");
        var a0 = new SavingsAccount("PL1", "PLN");
        var a1 = new CheckingAccount("PL2", "PLN");
        c.AddAccount(a0);
        c.AddAccount(a1);
        Assert.Same(a1, c[1]);
        Assert.Equal("Jan Kowalski", c.FullName);
    }

    [Fact]
    public void Bank_Indexer_ReturnsAccountByNumber()
    {
        var bank = new Bank("MójBank");
        var a = new SavingsAccount("PL999", "PLN");
        bank.Register(a);
        Assert.Same(a, bank["PL999"]);
    }

    [Fact]
    public void Bank_Indexer_UnknownNumber_Throws()
    {
        var bank = new Bank("MójBank");
        Assert.Throws<AccountNotFoundException>(() => bank["BRAK"]);
    }
}
