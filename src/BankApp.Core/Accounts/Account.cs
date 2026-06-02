using BankApp.Core.Attributes;

namespace BankApp.Core.Accounts;

public abstract class Account
{
    private readonly List<Transaction> _transactions = new();

    public int Id { get; set; }
    [ReportColumn("Numer konta")]
    public string Number { get; set; } = string.Empty;
    [ReportColumn("Waluta")]
    public string Currency { get; set; } = "PLN";
    [ReportColumn("Saldo")]
    public decimal BalanceAmount { get; set; }      // kolumna w bazie
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    /// <summary>Saldo jako typ wartości Money (właściwość wyliczana).</summary>
    public Money Balance => new(BalanceAmount, Currency);

    protected Account() { }                          // dla EF Core

    protected Account(string number, string currency)
        : this()                                     // łańcuchowanie konstruktorów (: this)
    {
        Number = number;
        Currency = currency;
    }

    /// <summary>Miesięczna opłata — różna dla każdego typu konta (polimorfizm).</summary>
    public abstract Money CalculateMonthlyFee();

    /// <summary>Czy można wypłacić kwotę. Domyślnie: nie wolno schodzić poniżej zera.</summary>
    public virtual bool CanWithdraw(Money amount) => Balance >= amount;

    public void Deposit(Money amount, string description = "Wpłata")
    {
        if (amount.Amount <= 0) throw new ArgumentException("Kwota musi być dodatnia.", nameof(amount));
        BalanceAmount += amount.Amount;
        _transactions.Add(new Transaction(TransactionType.Deposit, amount, description));
    }

    public void Withdraw(Money amount, string description = "Wypłata")
    {
        if (amount.Amount <= 0) throw new ArgumentException("Kwota musi być dodatnia.", nameof(amount));
        if (!CanWithdraw(amount)) throw new InsufficientFundsException(Number, Balance, amount);
        BalanceAmount -= amount.Amount;
        _transactions.Add(new Transaction(TransactionType.Withdrawal, amount, description));
    }

    /// <summary>Dodaje już zbudowaną transakcję i aktualizuje saldo (np. przelew, odsetki).</summary>
    public void Post(Transaction transaction)
    {
        _transactions.Add(transaction);
        BalanceAmount += transaction.SignedAmount;
    }

    public override string ToString() => $"{GetType().Name} {Number}: {Balance}";
}
