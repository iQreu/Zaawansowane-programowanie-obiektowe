using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankApp.Tests;

public class SqliteIntegrationTests
{
    [Fact]
    public async Task RoundTrip_PersistsAccountWithTransactions_OnRealSqlite()
    {
        // In-memory SQLite (real provider) needs an open connection kept alive.
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseSqlite(connection)
                .Options;

            await using (var ctx = new BankDbContext(options))
            {
                await ctx.Database.EnsureCreatedAsync();
                var savings = new SavingsAccount("PL100", "PLN") { AnnualInterestRate = 0.06m };
                savings.Deposit(new Money(1234.56m), "Wpłata");
                ctx.Accounts.Add(savings);
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = new BankDbContext(options))
            {
                var repo = new AccountRepository(ctx);
                var loaded = await repo.GetByNumberAsync("PL100");
                Assert.NotNull(loaded);
                Assert.IsType<SavingsAccount>(loaded);                 // TPH discriminator works
                Assert.Equal(1234.56m, loaded!.BalanceAmount);          // decimal-as-TEXT round-trips
                Assert.Single(loaded.Transactions);                    // backing-field navigation persisted
                Assert.Equal(0.06m, ((SavingsAccount)loaded).AnnualInterestRate);
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
