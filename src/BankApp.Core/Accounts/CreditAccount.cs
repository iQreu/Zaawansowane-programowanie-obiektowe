using BankApp.Core.Attributes;

namespace BankApp.Core.Accounts;

[AccountTypeInfo("Credit", "Karta kredytowa", "Pozwala na zadłużenie do limitu, nalicza odsetki od długu.")]
public sealed class CreditAccount : Account, IInterestBearing
{
    public decimal CreditLimit { get; set; } = 1000m;
    public decimal AnnualInterestRate { get; set; } = 0.20m;

    public CreditAccount() { }
    public CreditAccount(string number, string currency) : base(number, currency) { }

    public override bool CanWithdraw(Money amount) =>
        BalanceAmount - amount.Amount >= -CreditLimit;

    public override Money CalculateMonthlyFee() => new(0m, Currency);

    // Odsetki naliczane tylko od ujemnego salda (zadłużenia).
    public Money CalculateInterest() =>
        BalanceAmount < 0
            ? new Money(-BalanceAmount * AnnualInterestRate / 12m, Currency)
            : Money.Zero(Currency);
}
