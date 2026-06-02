namespace BankApp.Core.Services;

/// <summary>Dane zdarzenia wykonanej operacji (rekord = niezmienny DTO).</summary>
public record TransactionEventArgs(
    string AccountNumber,
    TransactionType Type,
    Money Amount,
    Money NewBalance);
