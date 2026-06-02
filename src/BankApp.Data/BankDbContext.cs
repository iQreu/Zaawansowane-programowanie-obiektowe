using BankApp.Core;
using BankApp.Core.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BankApp.Data;

public class BankDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Dziedziczenie kont jako Table-Per-Hierarchy (kolumna dyskryminatora).
        b.Entity<Account>()
            .HasDiscriminator<string>("AccountType")
            .HasValue<CheckingAccount>("Checking")
            .HasValue<SavingsAccount>("Savings")
            .HasValue<CreditAccount>("Credit");

        // Money to typ wyliczany — nie mapujemy.
        b.Entity<Account>().Ignore(a => a.Balance);
        b.Entity<Account>().Property(a => a.BalanceAmount).HasColumnType("TEXT");

        // Kolekcja transakcji jest read-only z prywatnym polem — EF mapuje przez pole.
        var nav = b.Entity<Account>().Metadata.FindNavigation(nameof(Account.Transactions));
        nav!.SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId);

        b.Entity<Transaction>().Ignore(t => t.Money);
        b.Entity<Transaction>().Ignore(t => t.SignedAmount);
        b.Entity<Transaction>().Property(t => t.Amount).HasColumnType("TEXT");

        // Kolekcja kont klienta — również przez pole.
        var custNav = b.Entity<Customer>().Metadata.FindNavigation(nameof(Customer.Accounts));
        custNav!.SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Entity<Customer>().Ignore(c => c.FullName);

        // CustomerId jest int (nie int?) — EF Core nie pozwoli oznaczyć go jako nullable.
        // Relację Customer->Accounts konfigurujemy przez shadow property "CustomerFk" (int?),
        // żeby FK kolumna w SQLite była NULL-owalna. CustomerId mapujemy osobno jako zwykłą kolumnę.
        b.Entity<Account>().Property<int?>("CustomerFk").HasColumnName("CustomerId");
        b.Entity<Customer>()
            .HasMany(c => c.Accounts)
            .WithOne(a => a.Customer!)
            .HasForeignKey("CustomerFk")
            .IsRequired(false);
    }
}
