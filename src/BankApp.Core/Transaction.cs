namespace BankApp.Core;

public class Transaction
{
    public int Id { get; set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public DateTime Timestamp { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int AccountId { get; set; }

    public Money Money => new(Amount, Currency);

    /// <summary>Kwota ze znakiem: wypłaty/opłaty/przelewy wychodzące są ujemne.</summary>
    public decimal SignedAmount => Type switch
    {
        TransactionType.Withdrawal or TransactionType.TransferOut or TransactionType.Fee => -Amount,
        _ => Amount
    };

    private Transaction() { } // dla EF Core

    public Transaction(TransactionType type, Money amount, string description)
    {
        Type = type;
        Amount = amount.Amount;
        Currency = amount.Currency;
        Description = description;
        Timestamp = DateTime.Now;
    }
}
