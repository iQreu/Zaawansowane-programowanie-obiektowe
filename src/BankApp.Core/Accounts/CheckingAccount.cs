using BankApp.Core.Attributes;

namespace BankApp.Core.Accounts;

[AccountTypeInfo("Checking", "Konto osobiste", "Konto codzienne z dopuszczalnym debetem.")]
public sealed class CheckingAccount : Account
{
    public decimal OverdraftLimit { get; set; } = 0m;

    public CheckingAccount() { }
    public CheckingAccount(string number, string currency) : base(number, currency) { }

    public override bool CanWithdraw(Money amount) =>
        BalanceAmount - amount.Amount >= -OverdraftLimit;

    public override Money CalculateMonthlyFee() => new(5m, Currency);
}
