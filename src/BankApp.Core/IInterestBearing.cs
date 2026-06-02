namespace BankApp.Core;

/// <summary>Konto, które nalicza odsetki.</summary>
public interface IInterestBearing
{
    decimal AnnualInterestRate { get; }
    Money CalculateInterest();
}
