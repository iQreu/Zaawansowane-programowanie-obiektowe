namespace BankApp.Core.Accounts;

/// <summary>Statyczna fabryka kont (switch expression + pattern matching).</summary>
public static class AccountFactory
{
    public static Account Create(string typeKey, string currency = "PLN")
    {
        var number = AccountNumberGenerator.Next();
        return typeKey switch
        {
            "Checking" => new CheckingAccount(number, currency),
            "Savings"  => new SavingsAccount(number, currency),
            "Credit"   => new CreditAccount(number, currency),
            _ => throw new ArgumentException($"Nieznany typ konta: {typeKey}", nameof(typeKey))
        };
    }
}
