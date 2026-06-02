namespace BankApp.Core.Interest;

public interface IInterestStrategy
{
    Money Calculate(IInterestBearing account);
}
