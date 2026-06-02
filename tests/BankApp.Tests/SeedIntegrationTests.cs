using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankApp.Tests;

public class SeedIntegrationTests
{
    [Fact]
    public async Task SeedAsync_TwoCustomers_With_CorrectlyLinkedAccounts_OnRealSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseSqlite(connection)
                .Options;

            // ---- SEED ----
            await using (var ctx = new BankDbContext(options))
            {
                await DbInitializer.SeedAsync(ctx);
            }

            // ---- RELOAD in fresh context ----
            await using (var ctx = new BankDbContext(options))
            {
                var customers = await ctx.Customers
                    .Include(c => c.Accounts)
                    .ToListAsync();

                // 1. There are 2 customers.
                Assert.Equal(2, customers.Count);

                // 2. "Jan Kowalski" has exactly 2 accounts.
                var jan = customers.Single(c => c.FirstName == "Jan" && c.LastName == "Kowalski");
                Assert.Equal(2, jan.Accounts.Count);

                // 3. Each reloaded account's CustomerId equals its owning customer's Id.
                Assert.True(jan.Id > 0, "Customer.Id must be > 0 after save (EF assigned PK).");
                foreach (var acc in jan.Accounts)
                {
                    Assert.True(acc.CustomerId == jan.Id,
                        $"Account {acc.Number}: CustomerId should equal customer Id {jan.Id}, got {acc.CustomerId}.");
                }

                var anna = customers.Single(c => c.FirstName == "Anna");
                foreach (var acc in anna.Accounts)
                {
                    Assert.True(acc.CustomerId == anna.Id,
                        $"Account {acc.Number}: CustomerId should equal customer Id {anna.Id}, got {acc.CustomerId}.");
                }

                // 4. At least one account reloads as the correct concrete type — Anna's is CreditAccount with CreditLimit 3000.
                var annaCredit = anna.Accounts.Single();
                Assert.IsType<CreditAccount>(annaCredit);
                Assert.Equal(3000m, ((CreditAccount)annaCredit).CreditLimit);
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
