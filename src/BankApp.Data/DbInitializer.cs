using BankApp.Core;
using BankApp.Core.Accounts;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BankDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Customers.AnyAsync()) return;

        var jan = new Customer("Jan", "Kowalski");
        var checking = new CheckingAccount(AccountNumberGenerator.Next(), "PLN") { OverdraftLimit = 200m };
        var savings  = new SavingsAccount(AccountNumberGenerator.Next(), "PLN") { AnnualInterestRate = 0.06m };
        checking.Deposit(new Money(1500m), "Wpłata początkowa");
        savings.Deposit(new Money(5000m), "Wpłata początkowa");
        jan.AddAccount(checking);
        jan.AddAccount(savings);

        var anna = new Customer("Anna", "Nowak");
        var credit = new CreditAccount(AccountNumberGenerator.Next(), "PLN") { CreditLimit = 3000m };
        anna.AddAccount(credit);

        db.Customers.AddRange(jan, anna);
        await db.SaveChangesAsync();
    }
}
