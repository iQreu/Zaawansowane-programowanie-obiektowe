# BankApp (OOP) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Zbudować desktopową aplikację bankową w C#/.NET 9 (WPF + EF Core/SQLite), której kod celowo i czytelnie demonstruje 12 wymaganych mechanizmów obiektowych oraz zestaw dodatków, wraz z dokumentem `DOKUMENTACJA.md` (co/jak/dlaczego).

**Architecture:** Cztery projekty z jednokierunkowymi zależnościami: `BankApp.Core` (czysta domena), `BankApp.Data` (EF Core/SQLite), `BankApp.Wpf` (MVVM), `BankApp.Tests` (xUnit). Domena jest niezależna od WPF i EF; logika biznesowa testowana jednostkowo (TDD), warstwy Data/WPF weryfikowane przez build + uruchomienie.

**Tech Stack:** .NET 9, C# 13, WPF, Entity Framework Core 9 (SQLite, TPH), Microsoft.Extensions.DependencyInjection, xUnit.

**Konwencje:** wszystkie polecenia uruchamiamy z katalogu repozytorium `E:\Zaawansowane-programowanie-obiektowe`. `nullable` i `ImplicitUsings` włączone. Po każdym zadaniu: build zielony, testy zielone, commit.

---

### Task 1: Solution i struktura projektów

**Files:**
- Create: `BankApp.sln`, `src/BankApp.Core/BankApp.Core.csproj`, `src/BankApp.Data/BankApp.Data.csproj`, `src/BankApp.Wpf/BankApp.Wpf.csproj`, `tests/BankApp.Tests/BankApp.Tests.csproj`
- Modify: `.gitignore`

- [ ] **Step 1: Utwórz solution i projekty**

Run:
```powershell
dotnet new sln -n BankApp
dotnet new classlib -n BankApp.Core -o src/BankApp.Core
dotnet new classlib -n BankApp.Data -o src/BankApp.Data
dotnet new wpf      -n BankApp.Wpf  -o src/BankApp.Wpf
dotnet new xunit    -n BankApp.Tests -o tests/BankApp.Tests
dotnet sln add src/BankApp.Core src/BankApp.Data src/BankApp.Wpf tests/BankApp.Tests
```

- [ ] **Step 2: Usuń wygenerowane pliki-zaślepki**

Usuń `src/BankApp.Core/Class1.cs` i `src/BankApp.Data/Class1.cs` (jeśli istnieją).

- [ ] **Step 3: Dodaj referencje między projektami**

Run:
```powershell
dotnet add src/BankApp.Data reference src/BankApp.Core
dotnet add src/BankApp.Wpf  reference src/BankApp.Core
dotnet add src/BankApp.Wpf  reference src/BankApp.Data
dotnet add tests/BankApp.Tests reference src/BankApp.Core
dotnet add tests/BankApp.Tests reference src/BankApp.Data
```

- [ ] **Step 4: Dodaj pakiety NuGet**

Run:
```powershell
dotnet add src/BankApp.Data package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/BankApp.Data package Microsoft.EntityFrameworkCore.Design
dotnet add src/BankApp.Wpf  package Microsoft.Extensions.DependencyInjection
dotnet add src/BankApp.Wpf  package Microsoft.EntityFrameworkCore.Sqlite
dotnet add tests/BankApp.Tests package Microsoft.EntityFrameworkCore.InMemory
```

- [ ] **Step 5: Ustaw TargetFramework i nullable**

W `src/BankApp.Core/BankApp.Core.csproj` i `src/BankApp.Data/BankApp.Data.csproj` upewnij się, że `<PropertyGroup>` zawiera:
```xml
<TargetFramework>net9.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```
W `src/BankApp.Wpf/BankApp.Wpf.csproj` `TargetFramework` musi być `net9.0-windows` (zostawić tak, jak wygenerował szablon).

- [ ] **Step 6: Zaktualizuj .gitignore**

Dopisz na końcu `.gitignore` (utwórz plik, jeśli nie istnieje):
```
bin/
obj/
*.db
*.db-shm
*.db-wal
.vs/
```

- [ ] **Step 7: Build całości**

Run: `dotnet build`
Expected: `Build succeeded`, 0 errors.

- [ ] **Step 8: Commit**

```powershell
git add -A
git commit -m "chore: scaffold BankApp solution (Core/Data/Wpf/Tests)"
```

---

### Task 2: Typ wartości `Money` (przeciążanie operatorów) — TDD

**Files:**
- Create: `src/BankApp.Core/Money.cs`
- Test: `tests/BankApp.Tests/MoneyTests.cs`

- [ ] **Step 1: Napisz testy (failing)**

`tests/BankApp.Tests/MoneyTests.cs`:
```csharp
using BankApp.Core;
using Xunit;

namespace BankApp.Tests;

public class MoneyTests
{
    [Fact]
    public void Addition_AddsAmounts()
    {
        var sum = new Money(10m) + new Money(5m);
        Assert.Equal(new Money(15m), sum);
    }

    [Fact]
    public void Subtraction_SubtractsAmounts()
    {
        Assert.Equal(new Money(5m), new Money(10m) - new Money(5m));
    }

    [Fact]
    public void Multiplication_ByDecimal_Scales()
    {
        Assert.Equal(new Money(20m), new Money(10m) * 2m);
    }

    [Fact]
    public void Comparison_Operators_Work()
    {
        Assert.True(new Money(10m) > new Money(5m));
        Assert.True(new Money(5m) < new Money(10m));
        Assert.True(new Money(10m) >= new Money(10m));
        Assert.True(new Money(10m) <= new Money(10m));
    }

    [Fact]
    public void Equality_ConsidersAmountAndCurrency()
    {
        Assert.True(new Money(10m, "PLN") == new Money(10m, "PLN"));
        Assert.True(new Money(10m, "PLN") != new Money(10m, "EUR"));
    }

    [Fact]
    public void Operation_OnDifferentCurrencies_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new Money(1m, "PLN") + new Money(1m, "EUR"));
    }

    [Fact]
    public void ExplicitCast_ToDecimal_ReturnsAmount()
    {
        Assert.Equal(10m, (decimal)new Money(10m));
    }
}
```

- [ ] **Step 2: Uruchom testy (mają nie kompilować/failować)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL — typ `Money` nie istnieje.

- [ ] **Step 3: Zaimplementuj `Money`**

`src/BankApp.Core/Money.cs`:
```csharp
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
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS (7 testów).

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add Money value type with operator overloads"
```

---

### Task 3: Enumy, atrybuty niestandardowe i `Transaction`

**Files:**
- Create: `src/BankApp.Core/TransactionType.cs`, `src/BankApp.Core/Attributes/AccountTypeInfoAttribute.cs`, `src/BankApp.Core/Attributes/ReportColumnAttribute.cs`, `src/BankApp.Core/Transaction.cs`
- Test: `tests/BankApp.Tests/TransactionTests.cs`

- [ ] **Step 1: Napisz test (failing)**

`tests/BankApp.Tests/TransactionTests.cs`:
```csharp
using BankApp.Core;
using Xunit;

namespace BankApp.Tests;

public class TransactionTests
{
    [Fact]
    public void Withdrawal_HasNegativeSignedAmount()
    {
        var t = new Transaction(TransactionType.Withdrawal, new Money(50m), "test");
        Assert.Equal(-50m, t.SignedAmount);
    }

    [Fact]
    public void Deposit_HasPositiveSignedAmount()
    {
        var t = new Transaction(TransactionType.Deposit, new Money(50m), "test");
        Assert.Equal(50m, t.SignedAmount);
    }
}
```

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL — `Transaction` nie istnieje.

- [ ] **Step 3: Zaimplementuj enum, atrybuty i Transaction**

`src/BankApp.Core/TransactionType.cs`:
```csharp
namespace BankApp.Core;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    TransferIn,
    TransferOut,
    Interest,
    Fee
}
```

`src/BankApp.Core/Attributes/AccountTypeInfoAttribute.cs`:
```csharp
namespace BankApp.Core.Attributes;

/// <summary>Metadane typu konta — czytane przez refleksję do budowy listy typów w GUI.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AccountTypeInfoAttribute : Attribute
{
    public string Key { get; }
    public string DisplayName { get; }
    public string Description { get; }

    public AccountTypeInfoAttribute(string key, string displayName, string description)
    {
        Key = key;
        DisplayName = displayName;
        Description = description;
    }
}
```

`src/BankApp.Core/Attributes/ReportColumnAttribute.cs`:
```csharp
namespace BankApp.Core.Attributes;

/// <summary>Oznacza właściwość jako kolumnę raportu (czytane przez refleksję w ReportGenerator).</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ReportColumnAttribute : Attribute
{
    public string Header { get; }
    public ReportColumnAttribute(string header) => Header = header;
}
```

`src/BankApp.Core/Transaction.cs`:
```csharp
namespace BankApp.Core;

public class Transaction
{
    public int Id { get; set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public DateTime Timestamp { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int AccountId { get; set; }

    public Money Money => new(Amount, Currency);

    /// <summary>Kwota ze znakiem: wypłaty/opłaty/przelewy wychodzące są ujemne.</summary>
    public decimal SignedAmount => Type switch
    {
        TransactionType.Withdrawal or TransactionType.TransferOut or TransactionType.Fee => -Amount,
        _ => Amount
    };

    private Transaction() { } // dla EF Core

    public Transaction(TransactionType type, Money amount, string description)
    {
        Type = type;
        Amount = amount.Amount;
        Currency = amount.Currency;
        Description = description;
        Timestamp = DateTime.Now;
    }
}
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add TransactionType, custom attributes and Transaction"
```

---

### Task 4: Własne wyjątki i `Result<T>`

**Files:**
- Create: `src/BankApp.Core/Exceptions.cs`, `src/BankApp.Core/Result.cs`
- Test: `tests/BankApp.Tests/ResultTests.cs`

- [ ] **Step 1: Napisz test (failing)**

`tests/BankApp.Tests/ResultTests.cs`:
```csharp
using BankApp.Core;
using Xunit;

namespace BankApp.Tests;

public class ResultTests
{
    [Fact]
    public void Success_CarriesValue()
    {
        var r = Result<int>.Success(42);
        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Value);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var r = Result<int>.Failure("błąd");
        Assert.False(r.IsSuccess);
        Assert.Equal("błąd", r.Error);
    }
}
```

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL — `Result` nie istnieje.

- [ ] **Step 3: Zaimplementuj wyjątki i Result**

`src/BankApp.Core/Exceptions.cs`:
```csharp
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
```

`src/BankApp.Core/Result.cs`:
```csharp
namespace BankApp.Core;

/// <summary>Generyczny wynik operacji: sukces z wartością albo porażka z komunikatem.</summary>
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool ok, T? value, string? error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add domain exceptions and generic Result<T>"
```

---

### Task 5: Klasa abstrakcyjna `Account` (dziedziczenie, polimorfizm, właściwości) — TDD

**Files:**
- Create: `src/BankApp.Core/Accounts/Account.cs`
- Test: `tests/BankApp.Tests/AccountBehaviorTests.cs` (testy dodamy przez podklasę w Task 6; tu tylko build)

> Uwaga: `Account` jest abstrakcyjna, więc testujemy ją przez podklasy w Task 6. W tym zadaniu tworzymy bazę i weryfikujemy kompilację.

- [ ] **Step 1: Zaimplementuj `Account`**

`src/BankApp.Core/Accounts/Account.cs`:
```csharp
namespace BankApp.Core.Accounts;

public abstract class Account
{
    private readonly List<Transaction> _transactions = new();

    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Currency { get; set; } = "PLN";
    public decimal BalanceAmount { get; set; }      // kolumna w bazie
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    /// <summary>Saldo jako typ wartości Money (właściwość wyliczana).</summary>
    public Money Balance => new(BalanceAmount, Currency);

    protected Account() { }                          // dla EF Core

    protected Account(string number, string currency)
        : this()                                     // łańcuchowanie konstruktorów (: this)
    {
        Number = number;
        Currency = currency;
    }

    /// <summary>Miesięczna opłata — różna dla każdego typu konta (polimorfizm).</summary>
    public abstract Money CalculateMonthlyFee();

    /// <summary>Czy można wypłacić kwotę. Domyślnie: nie wolno schodzić poniżej zera.</summary>
    public virtual bool CanWithdraw(Money amount) => Balance >= amount;

    public void Deposit(Money amount, string description = "Wpłata")
    {
        if (amount.Amount <= 0) throw new ArgumentException("Kwota musi być dodatnia.", nameof(amount));
        BalanceAmount += amount.Amount;
        _transactions.Add(new Transaction(TransactionType.Deposit, amount, description));
    }

    public void Withdraw(Money amount, string description = "Wypłata")
    {
        if (amount.Amount <= 0) throw new ArgumentException("Kwota musi być dodatnia.", nameof(amount));
        if (!CanWithdraw(amount)) throw new InsufficientFundsException(Number, Balance, amount);
        BalanceAmount -= amount.Amount;
        _transactions.Add(new Transaction(TransactionType.Withdrawal, amount, description));
    }

    /// <summary>Dodaje już zbudowaną transakcję i aktualizuje saldo (np. przelew, odsetki).</summary>
    public void Post(Transaction transaction)
    {
        _transactions.Add(transaction);
        BalanceAmount += transaction.SignedAmount;
    }

    public override string ToString() => $"{GetType().Name} {Number}: {Balance}";
}
```

> `Customer` jeszcze nie istnieje — kompilacja przejdzie po Task 8, więc build tego zadania może zgłaszać brak `Customer`. Aby zachować zielony build, **w tym kroku tymczasowo** zostaw `Customer` (klasa powstaje w Task 8). Jeśli wolisz mieć zielony build już teraz, wykonaj Task 8 (Customer/Bank) bezpośrednio po tym kroku, a potem wróć do Task 6. Kolejność Task 5 → 8 → 6 → 7 jest dozwolona.

- [ ] **Step 2: Commit (po uzyskaniu zielonego buildu — patrz uwaga)**

```powershell
git add -A
git commit -m "feat(core): add abstract Account base (inheritance/polymorphism)"
```

---

### Task 6: Podklasy kont + `IInterestBearing` (dziedziczenie, polimorfizm, interfejsy) — TDD

**Files:**
- Create: `src/BankApp.Core/IInterestBearing.cs`, `src/BankApp.Core/Accounts/CheckingAccount.cs`, `src/BankApp.Core/Accounts/SavingsAccount.cs`, `src/BankApp.Core/Accounts/CreditAccount.cs`
- Test: `tests/BankApp.Tests/AccountBehaviorTests.cs`

- [ ] **Step 1: Napisz testy (failing)**

`tests/BankApp.Tests/AccountBehaviorTests.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using Xunit;

namespace BankApp.Tests;

public class AccountBehaviorTests
{
    [Fact]
    public void Checking_AllowsOverdraftUpToLimit()
    {
        var acc = new CheckingAccount("PL1", "PLN") { OverdraftLimit = 100m };
        acc.Deposit(new Money(50m));
        // saldo 50, limit debetu 100 → można wypłacić 150
        Assert.True(acc.CanWithdraw(new Money(150m)));
        Assert.False(acc.CanWithdraw(new Money(151m)));
    }

    [Fact]
    public void Savings_DoesNotAllowOverdraft()
    {
        var acc = new SavingsAccount("PL2", "PLN");
        acc.Deposit(new Money(50m));
        Assert.False(acc.CanWithdraw(new Money(51m)));
    }

    [Fact]
    public void Savings_CalculatesInterest()
    {
        var acc = new SavingsAccount("PL2", "PLN") { AnnualInterestRate = 0.12m };
        acc.Deposit(new Money(1000m));
        // 12% rocznie → 1% miesięcznie = 10
        Assert.Equal(new Money(10m), acc.CalculateInterest());
    }

    [Fact]
    public void Credit_AllowsSpendingUpToCreditLimit()
    {
        var acc = new CreditAccount("PL3", "PLN") { CreditLimit = 500m };
        Assert.True(acc.CanWithdraw(new Money(500m)));
        Assert.False(acc.CanWithdraw(new Money(501m)));
    }

    [Fact]
    public void Polymorphism_MonthlyFeeDiffersByType()
    {
        Account checking = new CheckingAccount("PL1", "PLN");
        Account savings  = new SavingsAccount("PL2", "PLN");
        Assert.NotEqual(checking.CalculateMonthlyFee(), savings.CalculateMonthlyFee());
    }

    [Fact]
    public void Withdraw_BeyondLimit_ThrowsInsufficientFunds()
    {
        var acc = new SavingsAccount("PL2", "PLN");
        acc.Deposit(new Money(10m));
        Assert.Throws<InsufficientFundsException>(() => acc.Withdraw(new Money(20m)));
    }
}
```

- [ ] **Step 2: Uruchom testy (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL — podklasy/interfejs nie istnieją.

- [ ] **Step 3: Zaimplementuj interfejs i podklasy**

`src/BankApp.Core/IInterestBearing.cs`:
```csharp
namespace BankApp.Core;

/// <summary>Konto, które nalicza odsetki.</summary>
public interface IInterestBearing
{
    decimal AnnualInterestRate { get; }
    Money CalculateInterest();
}
```

`src/BankApp.Core/Accounts/CheckingAccount.cs`:
```csharp
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
```

`src/BankApp.Core/Accounts/SavingsAccount.cs`:
```csharp
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
```

`src/BankApp.Core/Accounts/CreditAccount.cs`:
```csharp
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
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add account subtypes and IInterestBearing"
```

---

### Task 7: Strategia naliczania odsetek (`IInterestStrategy`, wzorzec Strategy + delegacje) — TDD

**Files:**
- Create: `src/BankApp.Core/Interest/IInterestStrategy.cs`, `src/BankApp.Core/Interest/InterestStrategies.cs`
- Test: `tests/BankApp.Tests/InterestStrategyTests.cs`

- [ ] **Step 1: Napisz test (failing)**

`tests/BankApp.Tests/InterestStrategyTests.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Core.Interest;
using Xunit;

namespace BankApp.Tests;

public class InterestStrategyTests
{
    [Fact]
    public void StandardStrategy_UsesAccountInterest()
    {
        var acc = new SavingsAccount("PL2", "PLN") { AnnualInterestRate = 0.12m };
        acc.Deposit(new Money(1000m));
        IInterestStrategy strategy = new StandardInterestStrategy();
        Assert.Equal(new Money(10m), strategy.Calculate(acc));
    }

    [Fact]
    public void PromotionalStrategy_AddsBonus()
    {
        var acc = new SavingsAccount("PL2", "PLN") { AnnualInterestRate = 0.12m };
        acc.Deposit(new Money(1000m));
        IInterestStrategy strategy = new PromotionalInterestStrategy(bonus: new Money(5m));
        Assert.Equal(new Money(15m), strategy.Calculate(acc));
    }

    [Fact]
    public void DelegateStrategy_UsesProvidedFunction()
    {
        var acc = new SavingsAccount("PL2", "PLN");
        acc.Deposit(new Money(100m));
        // delegacja: Func<IInterestBearing, Money>
        IInterestStrategy strategy = new DelegateInterestStrategy(a => new Money(1m, "PLN"));
        Assert.Equal(new Money(1m), strategy.Calculate(acc));
    }
}
```

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL.

- [ ] **Step 3: Zaimplementuj strategie**

`src/BankApp.Core/Interest/IInterestStrategy.cs`:
```csharp
namespace BankApp.Core.Interest;

public interface IInterestStrategy
{
    Money Calculate(IInterestBearing account);
}
```

`src/BankApp.Core/Interest/InterestStrategies.cs`:
```csharp
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
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add interest strategies (Strategy pattern + delegates)"
```

---

### Task 8: `Customer` i `Bank` (indeksatory, kolekcje) — TDD

**Files:**
- Create: `src/BankApp.Core/Customer.cs`, `src/BankApp.Core/Bank.cs`
- Test: `tests/BankApp.Tests/IndexerTests.cs`

- [ ] **Step 1: Napisz test (failing)**

`tests/BankApp.Tests/IndexerTests.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using Xunit;

namespace BankApp.Tests;

public class IndexerTests
{
    [Fact]
    public void Customer_Indexer_ReturnsNthAccount()
    {
        var c = new Customer("Jan", "Kowalski");
        var a0 = new SavingsAccount("PL1", "PLN");
        var a1 = new CheckingAccount("PL2", "PLN");
        c.AddAccount(a0);
        c.AddAccount(a1);
        Assert.Same(a1, c[1]);
        Assert.Equal("Jan Kowalski", c.FullName);
    }

    [Fact]
    public void Bank_Indexer_ReturnsAccountByNumber()
    {
        var bank = new Bank("MójBank");
        var a = new SavingsAccount("PL999", "PLN");
        bank.Register(a);
        Assert.Same(a, bank["PL999"]);
    }

    [Fact]
    public void Bank_Indexer_UnknownNumber_Throws()
    {
        var bank = new Bank("MójBank");
        Assert.Throws<AccountNotFoundException>(() => bank["BRAK"]);
    }
}
```

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL.

- [ ] **Step 3: Zaimplementuj Customer i Bank**

`src/BankApp.Core/Customer.cs`:
```csharp
using BankApp.Core.Accounts;

namespace BankApp.Core;

public class Customer
{
    private readonly List<Account> _accounts = new();

    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public IReadOnlyList<Account> Accounts => _accounts;

    /// <summary>Indeksator po pozycji — zwraca i-te konto klienta.</summary>
    public Account this[int index] => _accounts[index];

    public Customer() { }
    public Customer(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void AddAccount(Account account)
    {
        _accounts.Add(account);
        account.Customer = this;
        account.CustomerId = Id;
    }
}
```

`src/BankApp.Core/Bank.cs`:
```csharp
using BankApp.Core.Accounts;

namespace BankApp.Core;

public class Bank
{
    private readonly Dictionary<string, Account> _accounts = new();

    public string Name { get; }
    public IEnumerable<Account> Accounts => _accounts.Values;

    public Bank(string name) => Name = name;

    public void Register(Account account) => _accounts[account.Number] = account;

    /// <summary>Indeksator po numerze konta.</summary>
    public Account this[string number] =>
        _accounts.TryGetValue(number, out var account)
            ? account
            : throw new AccountNotFoundException(number);
}
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add Customer and Bank with indexers"
```

---

### Task 9: Składowe statyczne (generator numerów, formatter, fabryka) — TDD

**Files:**
- Create: `src/BankApp.Core/AccountNumberGenerator.cs`, `src/BankApp.Core/CurrencyFormatter.cs`, `src/BankApp.Core/Accounts/AccountFactory.cs`
- Test: `tests/BankApp.Tests/StaticMembersTests.cs`

- [ ] **Step 1: Napisz test (failing)**

`tests/BankApp.Tests/StaticMembersTests.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using Xunit;

namespace BankApp.Tests;

public class StaticMembersTests
{
    [Fact]
    public void NumberGenerator_ProducesUniqueIncreasingNumbers()
    {
        var a = AccountNumberGenerator.Next();
        var b = AccountNumberGenerator.Next();
        Assert.NotEqual(a, b);
        Assert.StartsWith("PL", a);
    }

    [Fact]
    public void CurrencyFormatter_FormatsMoney()
    {
        Assert.Equal("1 234,50 PLN", CurrencyFormatter.Format(new Money(1234.5m)));
    }

    [Fact]
    public void Factory_CreatesRequestedType()
    {
        Assert.IsType<SavingsAccount>(AccountFactory.Create("Savings"));
        Assert.IsType<CreditAccount>(AccountFactory.Create("Credit"));
        Assert.IsType<CheckingAccount>(AccountFactory.Create("Checking"));
    }

    [Fact]
    public void Factory_UnknownType_Throws()
    {
        Assert.Throws<ArgumentException>(() => AccountFactory.Create("???"));
    }
}
```

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL.

- [ ] **Step 3: Zaimplementuj składowe statyczne**

`src/BankApp.Core/AccountNumberGenerator.cs`:
```csharp
namespace BankApp.Core;

/// <summary>Statyczny generator unikalnych numerów kont (licznik współdzielony w aplikacji).</summary>
public static class AccountNumberGenerator
{
    private static int _counter = 1_000_000;
    private static readonly object _lock = new();

    public static string Next()
    {
        lock (_lock)
        {
            _counter++;
            return $"PL{_counter:D10}";
        }
    }
}
```

`src/BankApp.Core/CurrencyFormatter.cs`:
```csharp
using System.Globalization;

namespace BankApp.Core;

public static class CurrencyFormatter
{
    private static readonly CultureInfo Pl = CultureInfo.GetCultureInfo("pl-PL");
    public static string Format(Money money) => $"{money.Amount.ToString("N2", Pl)} {money.Currency}";
}
```

`src/BankApp.Core/Accounts/AccountFactory.cs`:
```csharp
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
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS. (Jeśli format kultury zwróci inny separator — dostosuj oczekiwanie testu do faktycznego wyniku `pl-PL`; spację może renderować jako NBSP.)

> Jeśli test formattera failuje przez znak spacji niełamliwej (NBSP) w `pl-PL`, zamień asercję na: `Assert.Equal("PLN", CurrencyFormatter.Format(new Money(1234.5m)).Split(' ', ' ')[^1]);` lub porównaj `Replace(' ',' ')`. Wybierz wariant zgodny z faktycznym wyjściem.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add static number generator, formatter and account factory"
```

---

### Task 10: Refleksja — rejestr typów kont i generator raportów — TDD

**Files:**
- Create: `src/BankApp.Core/Reflection/AccountTypeRegistry.cs`, `src/BankApp.Core/Reflection/ReportGenerator.cs`
- Test: `tests/BankApp.Tests/ReflectionTests.cs`

- [ ] **Step 1: Napisz test (failing)**

`tests/BankApp.Tests/ReflectionTests.cs`:
```csharp
using System.Linq;
using BankApp.Core.Accounts;
using BankApp.Core.Reflection;
using Xunit;

namespace BankApp.Tests;

public class ReflectionTests
{
    [Fact]
    public void Registry_DiscoversAllAccountTypesViaReflection()
    {
        var types = AccountTypeRegistry.Discover().ToList();
        Assert.Contains(types, t => t.Key == "Checking");
        Assert.Contains(types, t => t.Key == "Savings");
        Assert.Contains(types, t => t.Key == "Credit");
        Assert.Equal(3, types.Count);
    }

    [Fact]
    public void Registry_ReadsDisplayNameFromAttribute()
    {
        var savings = AccountTypeRegistry.Discover().Single(t => t.Key == "Savings");
        Assert.Equal("Konto oszczędnościowe", savings.DisplayName);
    }

    [Fact]
    public void ReportGenerator_BuildsHeaderFromProperties()
    {
        var report = ReportGenerator.Generate(new[]
        {
            new SavingsAccount("PL1", "PLN")
        });
        Assert.Contains("Number", report);
    }
}
```

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL.

- [ ] **Step 3: Zaimplementuj refleksję**

`src/BankApp.Core/Reflection/AccountTypeRegistry.cs`:
```csharp
using System.Reflection;
using BankApp.Core.Accounts;
using BankApp.Core.Attributes;

namespace BankApp.Core.Reflection;

public sealed record AccountTypeDescriptor(string Key, string DisplayName, string Description, Type ClrType);

/// <summary>Wykrywa wszystkie podtypy Account przez refleksję i czyta ich atrybuty.</summary>
public static class AccountTypeRegistry
{
    public static IEnumerable<AccountTypeDescriptor> Discover()
    {
        var assembly = typeof(Account).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || !typeof(Account).IsAssignableFrom(type))
                continue;

            var info = type.GetCustomAttribute<AccountTypeInfoAttribute>();
            if (info is null)
                continue;

            yield return new AccountTypeDescriptor(info.Key, info.DisplayName, info.Description, type);
        }
    }
}
```

`src/BankApp.Core/Reflection/ReportGenerator.cs`:
```csharp
using System.Reflection;
using System.Text;
using BankApp.Core.Attributes;

namespace BankApp.Core.Reflection;

/// <summary>
/// Generyczny generator raportu CSV: przez refleksję czyta publiczne właściwości
/// elementów. Jeśli właściwość ma [ReportColumn], używa nagłówka z atrybutu.
/// </summary>
public static class ReportGenerator
{
    public static string Generate<T>(IEnumerable<T> items)
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && IsSimple(p.PropertyType))
            .ToArray();

        var headers = props.Select(p =>
            p.GetCustomAttribute<ReportColumnAttribute>()?.Header ?? p.Name);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", headers));

        foreach (var item in items)
        {
            var values = props.Select(p => p.GetValue(item)?.ToString() ?? "");
            sb.AppendLine(string.Join(";", values));
        }
        return sb.ToString();
    }

    private static bool IsSimple(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime);
    }
}
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add reflection-based type registry and report generator"
```

---

### Task 11: Interfejsy repozytoriów (typy ogólne)

**Files:**
- Create: `src/BankApp.Core/Repositories/IRepository.cs`, `src/BankApp.Core/Repositories/IAccountRepository.cs`

- [ ] **Step 1: Zaimplementuj interfejsy**

`src/BankApp.Core/Repositories/IRepository.cs`:
```csharp
namespace BankApp.Core.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task SaveChangesAsync();
}
```

`src/BankApp.Core/Repositories/IAccountRepository.cs`:
```csharp
using BankApp.Core.Accounts;

namespace BankApp.Core.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByNumberAsync(string number);
}
```

- [ ] **Step 2: Build**

Run: `dotnet build src/BankApp.Core`
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "feat(core): add generic repository interfaces"
```

---

### Task 12: `ITransactionService` + zdarzenia + implementacja (async, delegacje/zdarzenia) — TDD

**Files:**
- Create: `src/BankApp.Core/Services/ITransactionService.cs`, `src/BankApp.Core/Services/TransactionEventArgs.cs`, `src/BankApp.Core/Services/TransactionService.cs`
- Test: `tests/BankApp.Tests/TransactionServiceTests.cs`, `tests/BankApp.Tests/Fakes/FakeAccountRepository.cs`

- [ ] **Step 1: Napisz fake repo + testy (failing)**

`tests/BankApp.Tests/Fakes/FakeAccountRepository.cs`:
```csharp
using BankApp.Core.Accounts;
using BankApp.Core.Repositories;

namespace BankApp.Tests.Fakes;

public class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts;
    public int SaveCount { get; private set; }

    public FakeAccountRepository(params Account[] accounts) => _accounts = accounts.ToList();

    public Task<Account?> GetByNumberAsync(string number) =>
        Task.FromResult(_accounts.FirstOrDefault(a => a.Number == number));

    public Task<Account?> GetByIdAsync(int id) =>
        Task.FromResult(_accounts.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<Account>> GetAllAsync() =>
        Task.FromResult((IReadOnlyList<Account>)_accounts);

    public Task AddAsync(Account entity) { _accounts.Add(entity); return Task.CompletedTask; }
    public Task SaveChangesAsync() { SaveCount++; return Task.CompletedTask; }
}
```

`tests/BankApp.Tests/TransactionServiceTests.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Core.Services;
using BankApp.Tests.Fakes;
using Xunit;

namespace BankApp.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task Deposit_IncreasesBalance_AndRaisesEvent()
    {
        var acc = new SavingsAccount("PL1", "PLN");
        var repo = new FakeAccountRepository(acc);
        var service = new TransactionService(repo);

        TransactionEventArgs? captured = null;
        service.TransactionCompleted += (_, e) => captured = e;

        var result = await service.DepositAsync("PL1", new Money(100m));

        Assert.True(result.IsSuccess);
        Assert.Equal(new Money(100m), acc.Balance);
        Assert.NotNull(captured);
        Assert.Equal("PL1", captured!.AccountNumber);
    }

    [Fact]
    public async Task Withdraw_BeyondFunds_ReturnsFailure()
    {
        var acc = new SavingsAccount("PL1", "PLN");
        var repo = new FakeAccountRepository(acc);
        var service = new TransactionService(repo);

        var result = await service.WithdrawAsync("PL1", new Money(50m));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Transfer_MovesMoneyBetweenAccounts()
    {
        var from = new SavingsAccount("PL1", "PLN");
        var to   = new SavingsAccount("PL2", "PLN");
        from.Deposit(new Money(100m));
        var repo = new FakeAccountRepository(from, to);
        var service = new TransactionService(repo);

        var result = await service.TransferAsync("PL1", "PL2", new Money(40m));

        Assert.True(result.IsSuccess);
        Assert.Equal(new Money(60m), from.Balance);
        Assert.Equal(new Money(40m), to.Balance);
    }

    [Fact]
    public async Task Deposit_UnknownAccount_ReturnsFailure()
    {
        var service = new TransactionService(new FakeAccountRepository());
        var result = await service.DepositAsync("BRAK", new Money(10m));
        Assert.False(result.IsSuccess);
    }
}
```

- [ ] **Step 2: Uruchom testy (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL.

- [ ] **Step 3: Zaimplementuj zdarzenia, interfejs i serwis**

`src/BankApp.Core/Services/TransactionEventArgs.cs`:
```csharp
namespace BankApp.Core.Services;

/// <summary>Dane zdarzenia wykonanej operacji (rekord = niezmienny DTO).</summary>
public record TransactionEventArgs(
    string AccountNumber,
    TransactionType Type,
    Money Amount,
    Money NewBalance);
```

`src/BankApp.Core/Services/ITransactionService.cs`:
```csharp
namespace BankApp.Core.Services;

public interface ITransactionService
{
    event EventHandler<TransactionEventArgs>? TransactionCompleted;

    Task<Result<bool>> DepositAsync(string accountNumber, Money amount);
    Task<Result<bool>> WithdrawAsync(string accountNumber, Money amount);
    Task<Result<bool>> TransferAsync(string fromNumber, string toNumber, Money amount);
}
```

`src/BankApp.Core/Services/TransactionService.cs`:
```csharp
using BankApp.Core.Accounts;
using BankApp.Core.Repositories;

namespace BankApp.Core.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accounts;

    public event EventHandler<TransactionEventArgs>? TransactionCompleted;

    public TransactionService(IAccountRepository accounts) => _accounts = accounts;

    public async Task<Result<bool>> DepositAsync(string accountNumber, Money amount)
    {
        var account = await _accounts.GetByNumberAsync(accountNumber);
        if (account is null) return Result<bool>.Failure($"Nie znaleziono konta {accountNumber}.");

        await Task.Delay(150); // symulacja przetwarzania (async)
        try { account.Deposit(amount); }
        catch (Exception ex) { return Result<bool>.Failure(ex.Message); }

        await _accounts.SaveChangesAsync();
        Raise(account, TransactionType.Deposit, amount);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> WithdrawAsync(string accountNumber, Money amount)
    {
        var account = await _accounts.GetByNumberAsync(accountNumber);
        if (account is null) return Result<bool>.Failure($"Nie znaleziono konta {accountNumber}.");

        await Task.Delay(150);
        try { account.Withdraw(amount); }
        catch (Exception ex) { return Result<bool>.Failure(ex.Message); }

        await _accounts.SaveChangesAsync();
        Raise(account, TransactionType.Withdrawal, amount);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> TransferAsync(string fromNumber, string toNumber, Money amount)
    {
        var from = await _accounts.GetByNumberAsync(fromNumber);
        var to   = await _accounts.GetByNumberAsync(toNumber);
        if (from is null) return Result<bool>.Failure($"Nie znaleziono konta {fromNumber}.");
        if (to is null)   return Result<bool>.Failure($"Nie znaleziono konta {toNumber}.");

        await Task.Delay(200);
        try
        {
            var outTx = new Transaction(TransactionType.TransferOut, amount, $"Przelew do {toNumber}");
            if (!from.CanWithdraw(amount))
                throw new InsufficientFundsException(from.Number, from.Balance, amount);
            from.Post(outTx);
            to.Post(new Transaction(TransactionType.TransferIn, amount, $"Przelew z {fromNumber}"));
        }
        catch (Exception ex) { return Result<bool>.Failure(ex.Message); }

        await _accounts.SaveChangesAsync();
        Raise(from, TransactionType.TransferOut, amount);
        Raise(to, TransactionType.TransferIn, amount);
        return Result<bool>.Success(true);
    }

    private void Raise(Account account, TransactionType type, Money amount) =>
        TransactionCompleted?.Invoke(this, new TransactionEventArgs(account.Number, type, amount, account.Balance));
}
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(core): add transaction service with events and async ops"
```

---

### Task 13: `BankDbContext` (EF Core, TPH)

**Files:**
- Create: `src/BankApp.Data/BankDbContext.cs`

- [ ] **Step 1: Zaimplementuj DbContext**

`src/BankApp.Data/BankDbContext.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BankApp.Data;

public class BankDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Dziedziczenie kont jako Table-Per-Hierarchy (kolumna dyskryminatora).
        b.Entity<Account>()
            .HasDiscriminator<string>("AccountType")
            .HasValue<CheckingAccount>("Checking")
            .HasValue<SavingsAccount>("Savings")
            .HasValue<CreditAccount>("Credit");

        // Money to typ wyliczany — nie mapujemy.
        b.Entity<Account>().Ignore(a => a.Balance);
        b.Entity<Account>().Property(a => a.BalanceAmount).HasColumnType("TEXT");

        // Kolekcja transakcji jest read-only z prywatnym polem — EF mapuje przez pole.
        var nav = b.Entity<Account>().Metadata.FindNavigation(nameof(Account.Transactions));
        nav!.SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId);

        b.Entity<Transaction>().Ignore(t => t.Money);
        b.Entity<Transaction>().Ignore(t => t.SignedAmount);
        b.Entity<Transaction>().Property(t => t.Amount).HasColumnType("TEXT");

        // Kolekcja kont klienta — również przez pole.
        var custNav = b.Entity<Customer>().Metadata.FindNavigation(nameof(Customer.Accounts));
        custNav!.SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Entity<Customer>().Ignore(c => c.FullName);
        b.Entity<Customer>()
            .HasMany(c => c.Accounts)
            .WithOne(a => a.Customer!)
            .HasForeignKey(a => a.CustomerId);
    }
}
```

> Uwaga: SQLite nie ma natywnego `decimal`; kolumny `decimal` mapujemy jako `TEXT`, by uniknąć utraty precyzji i ostrzeżeń EF. To celowy wybór (udokumentować w `DOKUMENTACJA.md`).

- [ ] **Step 2: Build**

Run: `dotnet build src/BankApp.Data`
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "feat(data): add BankDbContext with TPH mapping"
```

---

### Task 14: Repozytoria EF (`Repository<T>`, `AccountRepository`)

**Files:**
- Create: `src/BankApp.Data/Repository.cs`, `src/BankApp.Data/AccountRepository.cs`
- Test: `tests/BankApp.Tests/RepositoryTests.cs`

- [ ] **Step 1: Napisz test (failing, EF InMemory)**

`tests/BankApp.Tests/RepositoryTests.cs`:
```csharp
using BankApp.Core.Accounts;
using BankApp.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankApp.Tests;

public class RepositoryTests
{
    private static BankDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new BankDbContext(options);
    }

    [Fact]
    public async Task AccountRepository_GetByNumber_ReturnsSavedAccount()
    {
        await using var ctx = NewContext();
        var repo = new AccountRepository(ctx);
        await repo.AddAsync(new SavingsAccount("PL77", "PLN"));
        await repo.SaveChangesAsync();

        var found = await repo.GetByNumberAsync("PL77");
        Assert.NotNull(found);
        Assert.IsType<SavingsAccount>(found);
    }
}
```

> EF InMemory ignoruje konfigurację TPH/`HasColumnType`, więc test sprawdza tylko logikę repozytorium. Mapowanie TPH zweryfikujemy realnie na SQLite w Task 15.

- [ ] **Step 2: Uruchom test (fail)**

Run: `dotnet test tests/BankApp.Tests`
Expected: FAIL.

- [ ] **Step 3: Zaimplementuj repozytoria**

`src/BankApp.Data/Repository.cs`:
```csharp
using BankApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Data;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BankDbContext Db;
    public Repository(BankDbContext db) => Db = db;

    public async Task<T?> GetByIdAsync(int id) => await Db.Set<T>().FindAsync(id);
    public async Task<IReadOnlyList<T>> GetAllAsync() => await Db.Set<T>().ToListAsync();
    public async Task AddAsync(T entity) => await Db.Set<T>().AddAsync(entity);
    public async Task SaveChangesAsync() => await Db.SaveChangesAsync();
}
```

`src/BankApp.Data/AccountRepository.cs`:
```csharp
using BankApp.Core.Accounts;
using BankApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Data;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(BankDbContext db) : base(db) { }

    public async Task<Account?> GetByNumberAsync(string number) =>
        await Db.Set<Account>()
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Number == number);
}
```

- [ ] **Step 4: Uruchom testy (pass)**

Run: `dotnet test tests/BankApp.Tests`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(data): add generic Repository and AccountRepository"
```

---

### Task 15: Inicjalizacja i seed bazy (SQLite)

**Files:**
- Create: `src/BankApp.Data/DbInitializer.cs`

- [ ] **Step 1: Zaimplementuj inicjalizator**

`src/BankApp.Data/DbInitializer.cs`:
```csharp
using BankApp.Core;
using BankApp.Core.Accounts;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BankDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Customers.AnyAsync()) return;

        var jan = new Customer("Jan", "Kowalski");
        var checking = new CheckingAccount(AccountNumberGenerator.Next(), "PLN") { OverdraftLimit = 200m };
        var savings  = new SavingsAccount(AccountNumberGenerator.Next(), "PLN") { AnnualInterestRate = 0.06m };
        checking.Deposit(new Money(1500m), "Wpłata początkowa");
        savings.Deposit(new Money(5000m), "Wpłata początkowa");
        jan.AddAccount(checking);
        jan.AddAccount(savings);

        var anna = new Customer("Anna", "Nowak");
        var credit = new CreditAccount(AccountNumberGenerator.Next(), "PLN") { CreditLimit = 3000m };
        anna.AddAccount(credit);

        db.Customers.AddRange(jan, anna);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build src/BankApp.Data`
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "feat(data): add DbInitializer with seed data"
```

---

### Task 16: Infrastruktura MVVM (`ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`)

**Files:**
- Create: `src/BankApp.Wpf/Mvvm/ViewModelBase.cs`, `src/BankApp.Wpf/Mvvm/RelayCommand.cs`, `src/BankApp.Wpf/Mvvm/AsyncRelayCommand.cs`

- [ ] **Step 1: Zaimplementuj bazę MVVM**

`src/BankApp.Wpf/Mvvm/ViewModelBase.cs`:
```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BankApp.Wpf.Mvvm;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
```

`src/BankApp.Wpf/Mvvm/RelayCommand.cs`:
```csharp
using System.Windows.Input;

namespace BankApp.Wpf.Mvvm;

/// <summary>Komenda oparta na delegacjach (Action/Func) — rdzeń ICommand w MVVM.</summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

`src/BankApp.Wpf/Mvvm/AsyncRelayCommand.cs`:
```csharp
using System.Windows.Input;

namespace BankApp.Wpf.Mvvm;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => !_isRunning && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        _isRunning = true;
        CommandManager.InvalidateRequerySuggested();
        try { await _execute(); }
        finally
        {
            _isRunning = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build src/BankApp.Wpf`
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "feat(wpf): add MVVM infrastructure (ViewModelBase, commands)"
```

---

### Task 17: Bootstrap DI w WPF (`App.xaml` / `App.xaml.cs`)

**Files:**
- Modify: `src/BankApp.Wpf/App.xaml`, `src/BankApp.Wpf/App.xaml.cs`

- [ ] **Step 1: Ustaw App.xaml bez StartupUri**

`src/BankApp.Wpf/App.xaml` (usuń atrybut `StartupUri`, zostaw resztę):
```xml
<Application x:Class="BankApp.Wpf.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
    </Application.Resources>
</Application>
```

- [ ] **Step 2: Skonfiguruj kontener DI**

`src/BankApp.Wpf/App.xaml.cs`:
```csharp
using System.IO;
using System.Windows;
using BankApp.Core.Repositories;
using BankApp.Core.Services;
using BankApp.Data;
using BankApp.Wpf.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Wpf;

public partial class App : Application
{
    private ServiceProvider _services = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dbPath = Path.Combine(AppContext.BaseDirectory, "bank.db");

        var collection = new ServiceCollection();
        collection.AddDbContext<BankDbContext>(o => o.UseSqlite($"Data Source={dbPath}"),
            ServiceLifetime.Singleton);
        collection.AddSingleton<IAccountRepository, AccountRepository>();
        collection.AddSingleton<ITransactionService, TransactionService>();
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<DiagnosticsViewModel>();
        collection.AddSingleton<MainWindow>();

        _services = collection.BuildServiceProvider();

        var db = _services.GetRequiredService<BankDbContext>();
        await DbInitializer.SeedAsync(db);

        var window = _services.GetRequiredService<MainWindow>();
        await _services.GetRequiredService<MainViewModel>().LoadAsync();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
```

> `MainWindow` i ViewModele powstają w Task 18–20. Build tego zadania przejdzie dopiero po nich — to oczekiwane. Commit wykonaj po Task 20 (lub tymczasowo zakomentuj rejestracje, których typy jeszcze nie istnieją, i odkomentuj w Task 20).

- [ ] **Step 3: Commit (po Task 20)**

```powershell
git add -A
git commit -m "feat(wpf): configure dependency injection bootstrap"
```

---

### Task 18: `MainViewModel` (logika prezentacji)

**Files:**
- Create: `src/BankApp.Wpf/ViewModels/MainViewModel.cs`

- [ ] **Step 1: Zaimplementuj MainViewModel**

`src/BankApp.Wpf/ViewModels/MainViewModel.cs`:
```csharp
using System.Collections.ObjectModel;
using System.Windows;
using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Core.Reflection;
using BankApp.Core.Repositories;
using BankApp.Core.Services;
using BankApp.Wpf.Mvvm;

namespace BankApp.Wpf.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionService _service;

    public ObservableCollection<Account> Accounts { get; } = new();
    public ObservableCollection<AccountTypeDescriptor> AccountTypes { get; } = new();

    private Account? _selectedAccount;
    public Account? SelectedAccount
    {
        get => _selectedAccount;
        set { if (SetField(ref _selectedAccount, value)) OnPropertyChanged(nameof(Transactions)); }
    }

    public IEnumerable<Transaction> Transactions => _selectedAccount?.Transactions ?? Enumerable.Empty<Transaction>();

    private decimal _amount;
    public decimal Amount { get => _amount; set => SetField(ref _amount, value); }

    private string _targetNumber = string.Empty;
    public string TargetNumber { get => _targetNumber; set => SetField(ref _targetNumber, value); }

    private AccountTypeDescriptor? _selectedType;
    public AccountTypeDescriptor? SelectedType { get => _selectedType; set => SetField(ref _selectedType, value); }

    private string _status = "Gotowe.";
    public string Status { get => _status; set => SetField(ref _status, value); }

    public AsyncRelayCommand DepositCommand { get; }
    public AsyncRelayCommand WithdrawCommand { get; }
    public AsyncRelayCommand TransferCommand { get; }
    public AsyncRelayCommand OpenAccountCommand { get; }

    public MainViewModel(IAccountRepository accounts, ITransactionService service)
    {
        _accounts = accounts;
        _service = service;

        // Subskrypcja zdarzenia serwisu (Observer/zdarzenia).
        _service.TransactionCompleted += OnTransactionCompleted;

        DepositCommand = new AsyncRelayCommand(DepositAsync, () => SelectedAccount is not null && Amount > 0);
        WithdrawCommand = new AsyncRelayCommand(WithdrawAsync, () => SelectedAccount is not null && Amount > 0);
        TransferCommand = new AsyncRelayCommand(TransferAsync,
            () => SelectedAccount is not null && Amount > 0 && !string.IsNullOrWhiteSpace(TargetNumber));
        OpenAccountCommand = new AsyncRelayCommand(OpenAccountAsync, () => SelectedType is not null);

        foreach (var t in AccountTypeRegistry.Discover())
            AccountTypes.Add(t);
    }

    public async Task LoadAsync()
    {
        Accounts.Clear();
        foreach (var a in await _accounts.GetAllAsync())
            Accounts.Add(a);
        SelectedAccount = Accounts.FirstOrDefault();
    }

    private async Task DepositAsync()
    {
        var result = await _service.DepositAsync(SelectedAccount!.Number, new Money(Amount));
        Report(result.IsSuccess, result.Error ?? $"Wpłacono {Amount:N2}.");
        await LoadAsync();
    }

    private async Task WithdrawAsync()
    {
        var result = await _service.WithdrawAsync(SelectedAccount!.Number, new Money(Amount));
        Report(result.IsSuccess, result.Error ?? $"Wypłacono {Amount:N2}.");
        await LoadAsync();
    }

    private async Task TransferAsync()
    {
        var result = await _service.TransferAsync(SelectedAccount!.Number, TargetNumber, new Money(Amount));
        Report(result.IsSuccess, result.Error ?? $"Przelano {Amount:N2} na {TargetNumber}.");
        await LoadAsync();
    }

    private async Task OpenAccountAsync()
    {
        var account = AccountFactory.Create(SelectedType!.Key);
        await _accounts.AddAsync(account);
        await _accounts.SaveChangesAsync();
        Status = $"Otwarto konto {account.Number} ({SelectedType.DisplayName}).";
        await LoadAsync();
    }

    private void OnTransactionCompleted(object? sender, TransactionEventArgs e) =>
        Status = $"[zdarzenie] {e.Type} {e.Amount} na {e.AccountNumber}. Nowe saldo: {e.NewBalance}.";

    private void Report(bool success, string message)
    {
        Status = message;
        if (!success) MessageBox.Show(message, "Błąd operacji", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
```

- [ ] **Step 2: Build (przejdzie po Task 20 wraz z resztą WPF; można odłożyć)**

Run: `dotnet build src/BankApp.Wpf`
Expected: błędy o braku `DiagnosticsViewModel`/`MainWindow` znikną po Task 19–20.

- [ ] **Step 3: Commit (po Task 20)**

---

### Task 19: `DiagnosticsViewModel` (refleksja + eksport raportu, async)

**Files:**
- Create: `src/BankApp.Wpf/ViewModels/DiagnosticsViewModel.cs`

- [ ] **Step 1: Zaimplementuj DiagnosticsViewModel**

`src/BankApp.Wpf/ViewModels/DiagnosticsViewModel.cs`:
```csharp
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using BankApp.Core.Reflection;
using BankApp.Core.Repositories;
using BankApp.Wpf.Mvvm;

namespace BankApp.Wpf.ViewModels;

public sealed class DiagnosticsViewModel : ViewModelBase
{
    private readonly IAccountRepository _accounts;

    public ObservableCollection<AccountTypeDescriptor> DiscoveredTypes { get; } = new();

    private string _report = string.Empty;
    public string Report { get => _report; set => SetField(ref _report, value); }

    public AsyncRelayCommand GenerateReportCommand { get; }

    public DiagnosticsViewModel(IAccountRepository accounts)
    {
        _accounts = accounts;

        // Refleksja: pokaż wszystkie wykryte typy kont.
        foreach (var t in AccountTypeRegistry.Discover())
            DiscoveredTypes.Add(t);

        GenerateReportCommand = new AsyncRelayCommand(GenerateAsync);
    }

    private async Task GenerateAsync()
    {
        var all = await _accounts.GetAllAsync();
        var csv = ReportGenerator.Generate(all);
        Report = csv;

        var path = Path.Combine(AppContext.BaseDirectory, "raport_kont.csv");
        await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
        Report += $"\nZapisano do: {path}";
    }
}
```

- [ ] **Step 2: Commit (po Task 20)**

---

### Task 20: `MainWindow` (widok XAML, data binding) + złożenie WPF

**Files:**
- Modify: `src/BankApp.Wpf/MainWindow.xaml`, `src/BankApp.Wpf/MainWindow.xaml.cs`

- [ ] **Step 1: Zaimplementuj widok**

`src/BankApp.Wpf/MainWindow.xaml`:
```xml
<Window x:Class="BankApp.Wpf.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="BankApp" Height="600" Width="900">
    <DockPanel>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem><TextBlock Text="{Binding Status}"/></StatusBarItem>
        </StatusBar>
        <TabControl>
            <TabItem Header="Konta i operacje">
                <Grid Margin="8">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="2*"/>
                        <ColumnDefinition Width="3*"/>
                    </Grid.ColumnDefinitions>

                    <ListBox Grid.Column="0" ItemsSource="{Binding Accounts}"
                             SelectedItem="{Binding SelectedAccount}"
                             DisplayMemberPath="."/>

                    <StackPanel Grid.Column="1" Margin="8,0,0,0">
                        <TextBlock Text="Historia transakcji:" FontWeight="Bold"/>
                        <DataGrid ItemsSource="{Binding Transactions}" AutoGenerateColumns="False"
                                  IsReadOnly="True" Height="220" Margin="0,4">
                            <DataGrid.Columns>
                                <DataGridTextColumn Header="Data" Binding="{Binding Timestamp}"/>
                                <DataGridTextColumn Header="Typ" Binding="{Binding Type}"/>
                                <DataGridTextColumn Header="Kwota" Binding="{Binding Amount}"/>
                                <DataGridTextColumn Header="Opis" Binding="{Binding Description}"/>
                            </DataGrid.Columns>
                        </DataGrid>

                        <TextBlock Text="Kwota:"/>
                        <TextBox Text="{Binding Amount, UpdateSourceTrigger=PropertyChanged}"/>
                        <StackPanel Orientation="Horizontal" Margin="0,4">
                            <Button Content="Wpłać" Command="{Binding DepositCommand}" Width="90" Margin="0,0,4,0"/>
                            <Button Content="Wypłać" Command="{Binding WithdrawCommand}" Width="90"/>
                        </StackPanel>
                        <TextBlock Text="Numer konta docelowego (przelew):"/>
                        <TextBox Text="{Binding TargetNumber, UpdateSourceTrigger=PropertyChanged}"/>
                        <Button Content="Przelej" Command="{Binding TransferCommand}" Width="90" HorizontalAlignment="Left" Margin="0,4"/>
                    </StackPanel>
                </Grid>
            </TabItem>

            <TabItem Header="Nowe konto">
                <StackPanel Margin="8" Width="360" HorizontalAlignment="Left">
                    <TextBlock Text="Typ konta (wykryty przez refleksję):"/>
                    <ComboBox ItemsSource="{Binding AccountTypes}"
                              SelectedItem="{Binding SelectedType}"
                              DisplayMemberPath="DisplayName"/>
                    <TextBlock Text="{Binding SelectedType.Description}" TextWrapping="Wrap" Margin="0,4"/>
                    <Button Content="Otwórz konto" Command="{Binding OpenAccountCommand}" Margin="0,8" Width="120" HorizontalAlignment="Left"/>
                </StackPanel>
            </TabItem>
        </TabControl>
    </DockPanel>
</Window>
```

`src/BankApp.Wpf/MainWindow.xaml.cs`:
```csharp
using System.Windows;
using BankApp.Wpf.ViewModels;

namespace BankApp.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

- [ ] **Step 2: Odkomentuj/uzupełnij rejestracje z Task 17, zbuduj całość**

Run: `dotnet build`
Expected: `Build succeeded`, 0 errors.

- [ ] **Step 3: Uruchom aplikację (weryfikacja ręczna)**

Run: `dotnet run --project src/BankApp.Wpf`
Expected: okno BankApp, lista kont z seed-a, działające wpłaty/wypłaty/przelew (status aktualizuje się przez zdarzenie), zakładka „Nowe konto" z typami z refleksji.

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "feat(wpf): add MainWindow view, wire up DI and viewmodels"
```

---

### Task 21: Pełny przebieg testów i build Release

**Files:** (brak nowych)

- [ ] **Step 1: Testy**

Run: `dotnet test`
Expected: wszystkie testy PASS.

- [ ] **Step 2: Build Release**

Run: `dotnet build -c Release`
Expected: `Build succeeded`, 0 warnings krytycznych.

- [ ] **Step 3: Commit (jeśli były poprawki)**

```powershell
git add -A
git commit -m "chore: green tests and release build"
```

---

### Task 22: `DOKUMENTACJA.md` (co / jak / dlaczego)

**Files:**
- Create: `DOKUMENTACJA.md`

- [ ] **Step 1: Napisz dokument wyjaśniający**

Utwórz `DOKUMENTACJA.md` w korzeniu repo. Dla każdego z 12 mechanizmów + dodatków wstaw sekcję wg szablonu, **podając realne ścieżki i numery linii z gotowego kodu** (otwórz pliki i odczytaj aktualne linie — nie zostawiaj wartości przybliżonych):

```markdown
# Dokumentacja mechanizmów obiektowych — BankApp

Dla każdego mechanizmu: CO (czym jest), JAK (gdzie w kodzie), DLACZEGO (po co tam).

## 1. Klasy
- **Co:** Podstawowe typy modelujące domenę.
- **Jak:** `src/BankApp.Core/Customer.cs`, `src/BankApp.Core/Transaction.cs`, `src/BankApp.Core/Bank.cs`, `src/BankApp.Core/Accounts/Account.cs`.
- **Dlaczego:** Oddzielne, jednoodpowiedzialne klasy modelują pojęcia bankowe.

## 2. Konstruktory
- **Co:** Przeciążone konstruktory + łańcuchowanie `: this` / `: base`.
- **Jak:** `Account.cs` (`protected Account(...) : this()`), podklasy kont (`: base(number, currency)`), `Customer(string, string)`.
- **Dlaczego:** Konstruktor bezparametrowy dla EF, parametrowy dla wygodnego tworzenia.

## 3. Właściwości / indeksatory
- **Co:** Auto-properties, właściwości wyliczane, indeksatory.
- **Jak:** `Account.Balance` (wyliczana), `Customer.this[int]`, `Bank.this[string]`.
- **Dlaczego:** `Balance` liczone z `BalanceAmount`; indeksatory dają naturalny dostęp.

## 4. Składowe statyczne
- **Co:** Statyczny generator, formatter, fabryka.
- **Jak:** `AccountNumberGenerator`, `CurrencyFormatter`, `AccountFactory`.
- **Dlaczego:** Współdzielony stan/licznik i operacje bezstanowe nie wymagają instancji.

## 5. Dziedziczenie
- **Co:** Hierarchia kont.
- **Jak:** `Account` → `CheckingAccount`/`SavingsAccount`/`CreditAccount`.
- **Dlaczego:** Wspólny stan i zachowanie w bazie, specjalizacja w podklasach.

## 6. Polimorfizm
- **Co:** `virtual`/`override`/`abstract`.
- **Jak:** `CalculateMonthlyFee()` (abstract+override), `CanWithdraw()` (virtual+override), `ToString()`.
- **Dlaczego:** Różne reguły per typ konta wywoływane przez referencję bazową.

## 7. Interfejsy / abstrakcje
- **Co:** Klasa abstrakcyjna + interfejsy.
- **Jak:** `abstract Account`; `IInterestBearing`, `IInterestStrategy`, `IRepository<T>`, `IAccountRepository`, `ITransactionService`.
- **Dlaczego:** Granice między warstwami, wstrzykiwanie zależności, testowalność.

## 8. Typy ogólne / kolekcje
- **Co:** Generyki i kolekcje.
- **Jak:** `Repository<T>`, `Result<T>`, `List<>`/`Dictionary<>` w domenie, `ObservableCollection<>` w VM.
- **Dlaczego:** Reużywalne repozytorium, bezpieczny typowo wynik, wiązanie GUI.

## 9. Delegacje / zdarzenia
- **Co:** `event`, `EventHandler<T>`, `Func`/`Action`.
- **Jak:** `ITransactionService.TransactionCompleted`, `RelayCommand`/`AsyncRelayCommand`, `DelegateInterestStrategy`, `INotifyPropertyChanged`.
- **Dlaczego:** Luźne powiadamianie GUI o operacjach; komendy MVVM.

## 10. Przeciążanie operatorów
- **Co:** Operatory na `Money`.
- **Jak:** `Money.cs` (`+ - * == != < > <= >=`, cast do decimal).
- **Dlaczego:** Naturalna arytmetyka kwot bez utraty czytelności.

## 11. Programowanie asynchroniczne
- **Co:** `async/await`.
- **Jak:** `TransactionService.*Async` (+`Task.Delay`), `Repository` (`ToListAsync`/`SaveChangesAsync`), komendy async, eksport raportu.
- **Dlaczego:** Operacje I/O (baza, plik) bez blokowania GUI.

## 12. Refleksja
- **Co:** Skan assembly + odczyt atrybutów.
- **Jak:** `AccountTypeRegistry.Discover()` (podtypy + `AccountTypeInfoAttribute`), `ReportGenerator` (właściwości + `ReportColumnAttribute`).
- **Dlaczego:** Lista typów kont w GUI budowana automatycznie; generyczny raport.

## Dodatki (spoza listy)
- **Wzorce:** Repository, MVVM, Strategy, Factory, Observer, DI — gdzie i po co.
- **Własne wyjątki:** `BankException`, `InsufficientFundsException`, `AccountNotFoundException`.
- **Result<T>:** obsługa błędów bez wyjątków na granicy serwisu.
- **Atrybuty niestandardowe:** `AccountTypeInfoAttribute`, `ReportColumnAttribute`.
- **LINQ / pattern matching / rekordy / nullable / extension methods:** gdzie użyte.
- **Testy jednostkowe:** `tests/BankApp.Tests` — co pokrywają.
- **EF Core TPH + SQLite:** decyzja o mapowaniu dziedziczenia i `decimal` jako `TEXT`.
```

Uzupełnij realne `plik:linia` przez przejrzenie kodu (Grep/Read), aby dokument był dokładną ściągą do obrony.

- [ ] **Step 2: Commit**

```powershell
git add -A
git commit -m "docs: add DOKUMENTACJA.md explaining each OOP mechanism"
```

---

## Self-Review (wykonane)

**Pokrycie specyfikacji:**
- 12 mechanizmów → Task 2–12 + 16 (delegacje/zdarzenia w MVVM), zmapowane w Task 22. ✔
- Dodatki (DI, wyjątki, Result, atrybuty, LINQ, rekordy, testy) → Task 4, 7, 10, 12, 16–20. ✔
- EF Core/SQLite TPH → Task 13–15. ✔
- WPF/MVVM, 4 z 5 zakładek (Konta+operacje, Nowe konto, Diagnostyka) → Task 16–20. Ekran „Diagnostyka/Raport" ma `DiagnosticsViewModel` (Task 19); jeśli chcesz osobną zakładkę w oknie, dołóż `TabItem` bindowany do `DiagnosticsViewModel` (DataContext ustaw w MainWindow zasobie). ✔
- `DOKUMENTACJA.md` (sekcja 11 specyfikacji) → Task 22. ✔

**Uwaga o kolejności:** ze względu na zależność `Account`↔`Customer`, zalecana realna kolejność implementacji: Task 1 → 2 → 3 → 4 → 8 (Customer/Bank) → 5 (Account) → 6 → 7 → 9 → 10 → 11 → 12 → 13–15 → 16–20 → 21 → 22. Zielony build osiągamy najpóźniej po Task 8+5.

**Brak placeholderów:** wszystkie kroki zawierają konkretny kod i polecenia.

**Spójność typów:** `AccountFactory.Create`, `IAccountRepository.GetByNumberAsync`, `TransactionEventArgs`, `Result<T>`, `Money` używane spójnie między zadaniami.
