namespace BankApp.Core.Interest;

/// <summary>Standardowa strategia — deleguje do logiki konta.</summary>
public sealed class StandardInterestStrategy : IInterestStrategy
{
    public Money Calculate(IInterestBearing account) => account.CalculateInterest();
}

/// <summary>Promocyjna — dodaje stały bonus do naliczonych odsetek.</summary>
public sealed class PromotionalInterestStrategy : IInterestStrategy
{
    private readonly Money _bonus;
    public PromotionalInterestStrategy(Money bonus) => _bonus = bonus;
    public Money Calculate(IInterestBearing account) => account.CalculateInterest() + _bonus;
}

/// <summary>Strategia oparta na delegacji — pozwala wstrzyknąć dowolną funkcję.</summary>
public sealed class DelegateInterestStrategy : IInterestStrategy
{
    private readonly Func<IInterestBearing, Money> _formula;
    public DelegateInterestStrategy(Func<IInterestBearing, Money> formula) => _formula = formula;
    public Money Calculate(IInterestBearing account) => _formula(account);
}
