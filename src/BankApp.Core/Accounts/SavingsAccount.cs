using BankApp.Core.Attributes;

namespace BankApp.Core.Accounts;

[AccountTypeInfo("Savings", "Konto oszczędnościowe", "Bez debetu, nalicza odsetki.")]
public sealed class SavingsAccount : Account, IInterestBearing
{
    public decimal AnnualInterestRate { get; set; } = 0.05m;

    public SavingsAccount() { }
    public SavingsAccount(string number, string currency) : base(number, currency) { }

    public override Money CalculateMonthlyFee() => Money.Zero(Currency);

    public Money CalculateInterest() => new(BalanceAmount * AnnualInterestRate / 12m, Currency);
}
