namespace BankApp.Core;

/// <summary>Niezmienny typ wartości reprezentujący kwotę pieniężną wraz z walutą.</summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "PLN")
    {
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static Money Zero(string currency = "PLN") => new(0m, currency);

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException(
                $"Nie można operować na różnych walutach: {a.Currency} i {b.Currency}.");
    }

    public static Money operator +(Money a, Money b) { EnsureSameCurrency(a, b); return new(a.Amount + b.Amount, a.Currency); }
    public static Money operator -(Money a, Money b) { EnsureSameCurrency(a, b); return new(a.Amount - b.Amount, a.Currency); }
    public static Money operator -(Money a) => new(-a.Amount, a.Currency);
    public static Money operator *(Money a, decimal factor) => new(a.Amount * factor, a.Currency);

    public static bool operator ==(Money a, Money b) => a.Amount == b.Amount && a.Currency == b.Currency;
    public static bool operator !=(Money a, Money b) => !(a == b);
    public static bool operator >(Money a, Money b)  { EnsureSameCurrency(a, b); return a.Amount > b.Amount; }
    public static bool operator <(Money a, Money b)  { EnsureSameCurrency(a, b); return a.Amount < b.Amount; }
    public static bool operator >=(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount >= b.Amount; }
    public static bool operator <=(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount <= b.Amount; }

    public static explicit operator decimal(Money m) => m.Amount;

    public int CompareTo(Money other) { EnsureSameCurrency(this, other); return Amount.CompareTo(other.Amount); }
    public bool Equals(Money other) => this == other;
    public override bool Equals(object? obj) => obj is Money m && Equals(m);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:N2} {Currency}";
}
