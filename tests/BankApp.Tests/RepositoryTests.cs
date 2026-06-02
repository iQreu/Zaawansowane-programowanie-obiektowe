using BankApp.Core.Accounts;
using BankApp.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankApp.Tests;

public class RepositoryTests
{
    private static BankDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new BankDbContext(options);
    }

    [Fact]
    public async Task AccountRepository_GetByNumber_ReturnsSavedAccount()
    {
        await using var ctx = NewContext();
        var repo = new AccountRepository(ctx);
        await repo.AddAsync(new SavingsAccount("PL77", "PLN"));
        await repo.SaveChangesAsync();

        var found = await repo.GetByNumberAsync("PL77");
        Assert.NotNull(found);
        Assert.IsType<SavingsAccount>(found);
    }
}
