namespace BankApp.Core;

public class BankException : Exception
{
    public BankException(string message) : base(message) { }
}

public sealed class InsufficientFundsException : BankException
{
    public InsufficientFundsException(string accountNumber, Money balance, Money requested)
        : base($"Konto {accountNumber}: niewystarczające środki. Saldo {balance}, żądano {requested}.") { }
}

public sealed class AccountNotFoundException : BankException
{
    public AccountNotFoundException(string accountNumber)
        : base($"Nie znaleziono konta {accountNumber}.") { }
}
