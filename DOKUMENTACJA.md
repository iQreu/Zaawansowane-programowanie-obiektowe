# BankApp — Dokumentacja mechanizmów OOP

## Jak uruchomić

```bash
dotnet build
dotnet test
dotnet run --project src/BankApp.Wpf
```

---

## 1. Klasy

**CO:** Klasa to podstawowy budulec OOP — szablon definiujący stan (pola/właściwości) i zachowanie (metody) obiektów.

**JAK:**

- `src/BankApp.Core/Accounts/Account.cs` — linia 3: `public abstract class Account`
- `src/BankApp.Core/Customer.cs` — linia 5: `public class Customer`
- `src/BankApp.Core/Transaction.cs` — linia 3: `public class Transaction`
- `src/BankApp.Core/Bank.cs` — linia 5: `public class Bank`

```csharp
// src/BankApp.Core/Accounts/Account.cs, linia 3
public abstract class Account
{
    private readonly List<Transaction> _transactions = new();
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    // ...
}
```

**DLACZEGO:** Każda klasa odpowiada jednemu pojęciu domenowemu (konto, klient, transakcja, bank). Podział na oddzielne pliki ułatwia utrzymanie i testowanie.

---

## 2. Konstruktory

**CO:** Konstruktor inicjuje nowy obiekt; łańcuchowanie (`: this()`, `: base(...)`) eliminuje powtórzenia kodu inicjalizacyjnego.

**JAK:**

- `src/BankApp.Core/Accounts/Account.cs` — linia 19 (bezparametrowy dla EF Core) i linia 21–25 (łańcuchowanie `: this()`):

```csharp
// src/BankApp.Core/Accounts/Account.cs, linie 19-26
protected Account() { }                          // dla EF Core

protected Account(string number, string currency)
    : this()                                     // łańcuchowanie konstruktorów (: this)
{
    Number = number;
    Currency = currency;
}
```

- `src/BankApp.Core/Accounts/CheckingAccount.cs` — linia 11: `public CheckingAccount(string number, string currency) : base(number, currency) { }`
- `src/BankApp.Core/Customer.cs` — linia 20–24: `public Customer(string firstName, string lastName)`

**DLACZEGO:** Bezparametrowy konstruktor jest wymagany przez Entity Framework Core do materializacji encji z bazy danych. Konstruktor z parametrami umożliwia tworzenie w pełni zainicjowanych obiektów w kodzie domenowym i testach. Łańcuchowanie `: this()` zapewnia, że logika EF-owa inicjalizacji wykonuje się zawsze.

---

## 3. Właściwości i indeksatory

**CO:** Właściwość to kontrolowany dostęp do stanu obiektu; indeksator umożliwia dostęp do elementów obiektu za pomocą operatora `[]`.

**JAK:**

- `src/BankApp.Core/Accounts/Account.cs` — linia 17: właściwość wyliczana `Balance`:

```csharp
// src/BankApp.Core/Accounts/Account.cs, linia 17
public Money Balance => new(BalanceAmount, Currency);
```

- `src/BankApp.Core/Customer.cs` — linia 17: indeksator po pozycji:

```csharp
// src/BankApp.Core/Customer.cs, linia 17
public Account this[int index] => _accounts[index];
```

- `src/BankApp.Core/Bank.cs` — linia 17–19: indeksator po numerze konta:

```csharp
// src/BankApp.Core/Bank.cs, linie 17-20
public Account this[string number] =>
    _accounts.TryGetValue(number, out var account)
        ? account
        : throw new AccountNotFoundException(number);
```

**DLACZEGO:** `Balance` jest właściwością wyliczaną (nie przechowywaną w bazie), aby `Money` z właściwą walutą był zawsze spójny z `BalanceAmount`. Indeksatory pozwalają na naturalne wyszukiwanie kont (np. `bank["PL0001234567"]`) bez udostępniania wewnętrznego słownika.

---

## 4. Składowe statyczne

**CO:** Składowe statyczne należą do klasy, a nie do instancji — służą do danych i metod współdzielonych przez cały program.

**JAK:**

- `src/BankApp.Core/AccountNumberGenerator.cs` — linia 4–17: statyczna klasa z atomowym licznikiem:

```csharp
// src/BankApp.Core/AccountNumberGenerator.cs, linie 4-16
public static class AccountNumberGenerator
{
    private static int _counter = 1_000_000;
    private static readonly object _lock = new();

    public static string Next()
    {
        lock (_lock) { _counter++; return $"PL{_counter:D10}"; }
    }
}
```

- `src/BankApp.Core/CurrencyFormatter.cs` — linia 5–9: statyczna klasa formatująca kwoty.
- `src/BankApp.Core/Accounts/AccountFactory.cs` — linia 4: `public static class AccountFactory` — statyczna fabryka kont.

**DLACZEGO:** Generator numerów kont musi gwarantować unikalność w całej sesji, więc licznik musi być globalny (statyczny) i chroniony lockiem. `CurrencyFormatter` i `AccountFactory` nie wymagają stanu instancji — statyczność jest tu naturalnym wyborem.

---

## 5. Dziedziczenie

**CO:** Dziedziczenie pozwala podklasie przejąć pola, właściwości i metody klasy bazowej, rozszerzając lub specjalizując jej zachowanie.

**JAK:**

- `src/BankApp.Core/Accounts/CheckingAccount.cs` — linia 6: `public sealed class CheckingAccount : Account`
- `src/BankApp.Core/Accounts/SavingsAccount.cs` — linia 6: `public sealed class SavingsAccount : Account, IInterestBearing`
- `src/BankApp.Core/Accounts/CreditAccount.cs` — linia 6: `public sealed class CreditAccount : Account, IInterestBearing`
- `src/BankApp.Data/AccountRepository.cs` — linia 7: `public class AccountRepository : Repository<Account>, IAccountRepository`

```csharp
// src/BankApp.Core/Accounts/SavingsAccount.cs, linia 6
public sealed class SavingsAccount : Account, IInterestBearing
{
    public decimal AnnualInterestRate { get; set; } = 0.05m;
    public SavingsAccount(string number, string currency) : base(number, currency) { }
    // ...
}
```

**DLACZEGO:** Wszystkie typy kont dzielą wspólną logikę Deposit/Withdraw/Post zdefiniowaną w `Account`. Każda podklasa dodaje jedynie specyficzne reguły (limit debetowy, oprocentowanie), nie duplikując kodu bazowego. `sealed` zapobiega dalszemu niezamierzonemu rozszerzaniu.

---

## 6. Polimorfizm

**CO:** Polimorfizm pozwala wywoływać różne implementacje tej samej metody w zależności od rzeczywistego typu obiektu w czasie wykonania.

**JAK:**

- `src/BankApp.Core/Accounts/Account.cs` — linia 29: metoda abstrakcyjna; linia 32: metoda wirtualna; linia 56: `override ToString`:

```csharp
// src/BankApp.Core/Accounts/Account.cs, linie 29-32
public abstract Money CalculateMonthlyFee();
public virtual bool CanWithdraw(Money amount) => Balance >= amount;
// linia 56:
public override string ToString() => $"{GetType().Name} {Number}: {Balance}";
```

- `src/BankApp.Core/Accounts/CheckingAccount.cs` — linia 13: `public override bool CanWithdraw(Money amount)` (umożliwia debet do limitu).
- `src/BankApp.Core/Accounts/CreditAccount.cs` — linia 14: `public override bool CanWithdraw(Money amount)` (umożliwia zadłużenie do limitu kredytowego).
- `src/BankApp.Core/Accounts/SavingsAccount.cs` — linia 13: `public override Money CalculateMonthlyFee() => Money.Zero(Currency)`.

**DLACZEGO:** `CalculateMonthlyFee` jest abstrakcyjna, bo każdy typ konta ma inną opłatę (5 zł dla konta osobistego, 0 zł dla oszczędnościowego). `CanWithdraw` jest wirtualna z domyślną implementacją, którą podklasy nadpisują tylko wtedy, gdy konieczne jest inne zachowanie (debet, kredyt).

---

## 7. Interfejsy i abstrakcje

**CO:** Interfejs definiuje kontrakt (zestaw metod/właściwości) bez implementacji; klasa abstrakcyjna może mieszać kontrakt z częściową implementacją.

**JAK:**

- `src/BankApp.Core/Accounts/Account.cs` — linia 3: `public abstract class Account`
- `src/BankApp.Core/IInterestBearing.cs` — linia 4: `public interface IInterestBearing`

```csharp
// src/BankApp.Core/IInterestBearing.cs, linie 4-8
public interface IInterestBearing
{
    decimal AnnualInterestRate { get; }
    Money CalculateInterest();
}
```

- `src/BankApp.Core/Interest/IInterestStrategy.cs` — linia 3: `public interface IInterestStrategy`
- `src/BankApp.Core/Repositories/IRepository.cs` — linia 3: `public interface IRepository<T> where T : class`
- `src/BankApp.Core/Repositories/IAccountRepository.cs` — linia 5: `public interface IAccountRepository : IRepository<Account>`
- `src/BankApp.Core/Services/ITransactionService.cs` — linia 3: `public interface ITransactionService`

**DLACZEGO:** Interfejsy `IRepository<T>` i `ITransactionService` umożliwiają wstrzykiwanie zależności (DI) i testowanie za pomocą atrap (np. `FakeAccountRepository`). `IInterestBearing` segreguje konta z oprocentowaniem od kont bez niego. Abstrakcyjna klasa `Account` definiuje wspólny kontrakt kont, jednocześnie udostępniając gotową implementację `Deposit`/`Withdraw`.

---

## 8. Typy ogólne i kolekcje

**CO:** Typy ogólne (generics) pozwalają pisać kod niezależny od konkretnego typu danych, zachowując bezpieczeństwo typów w czasie kompilacji.

**JAK:**

- `src/BankApp.Data/Repository.cs` — linia 6: `public class Repository<T> : IRepository<T> where T : class`

```csharp
// src/BankApp.Data/Repository.cs, linie 6-15
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BankDbContext Db;
    public Repository(BankDbContext db) => Db = db;

    public async Task<IReadOnlyList<T>> GetAllAsync() => await Db.Set<T>().ToListAsync();
    public async Task AddAsync(T entity) => await Db.Set<T>().AddAsync(entity);
    // ...
}
```

- `src/BankApp.Core/Result.cs` — linia 4: `public readonly struct Result<T>` — generyczny wynik operacji.
- `src/BankApp.Core/Accounts/Account.cs` — linia 5: `private readonly List<Transaction> _transactions = new()` (lista transakcji).
- `src/BankApp.Core/Bank.cs` — linia 7: `private readonly Dictionary<string, Account> _accounts = new()` (słownik kont).
- `src/BankApp.Wpf/ViewModels/MainViewModel.cs` — linia 17: `public ObservableCollection<Account> Accounts { get; } = new()` (reaktywna kolekcja WPF).

**DLACZEGO:** `Repository<T>` eliminuje duplikację kodu CRUD dla różnych encji — wystarczy jedna implementacja dla `Customer`, `Account`, `Transaction`. `Result<T>` pozwala zwracać wynik operacji wraz z komunikatem błędu bez rzucania wyjątków. `ObservableCollection<T>` automatycznie powiadamia widok WPF o zmianach listy.

---

## 9. Delegacje i zdarzenia

**CO:** Delegacja to typowany wskaźnik na metodę; zdarzenie (event) to mechanizm powiadamiania subskrybentów o zajściu faktu.

**JAK:**

- `src/BankApp.Core/Services/ITransactionService.cs` — linia 5: deklaracja zdarzenia:

```csharp
// src/BankApp.Core/Services/ITransactionService.cs, linia 5
event EventHandler<TransactionEventArgs>? TransactionCompleted;
```

- `src/BankApp.Core/Services/TransactionService.cs` — linia 10: implementacja; linia 67: podnoszenie zdarzenia (`?.Invoke`).
- `src/BankApp.Wpf/ViewModels/MainViewModel.cs` — linia 54: subskrypcja `_service.TransactionCompleted += OnTransactionCompleted`.
- `src/BankApp.Wpf/Mvvm/RelayCommand.cs` — linie 8–9: delegacje `Action` / `Func<bool>`:

```csharp
// src/BankApp.Wpf/Mvvm/RelayCommand.cs, linie 8-9
private readonly Action _execute;
private readonly Func<bool>? _canExecute;
```

- `src/BankApp.Core/Interest/InterestStrategies.cs` — linia 20: `DelegateInterestStrategy` z `Func<IInterestBearing, Money>`.
- `src/BankApp.Wpf/Mvvm/ViewModelBase.cs` — linia 8: `public event PropertyChangedEventHandler? PropertyChanged` (implementacja `INotifyPropertyChanged`).

**DLACZEGO:** Zdarzenie `TransactionCompleted` realizuje wzorzec Observer — ViewModel subskrybuje serwis bez silnego sprzężenia. `RelayCommand` przyjmuje `Action`/`Func` jako delegacje, co eliminuje konieczność tworzenia osobnych klas komendy. `DelegateInterestStrategy` umożliwia definiowanie wzorów oprocentowania jako wyrażeń lambda.

---

## 10. Przeciążanie operatorów

**CO:** Przeciążanie operatorów pozwala zdefiniować semantykę operatorów (`+`, `-`, `==`, `<` itp.) dla własnych typów.

**JAK:**

- `src/BankApp.Core/Money.cs` — linie 24–36: kompletny zestaw operatorów:

```csharp
// src/BankApp.Core/Money.cs, linie 24-36
public static Money operator +(Money a, Money b)  { EnsureSameCurrency(a, b); return new(a.Amount + b.Amount, a.Currency); }
public static Money operator -(Money a, Money b)  { EnsureSameCurrency(a, b); return new(a.Amount - b.Amount, a.Currency); }
public static Money operator *(Money a, decimal factor) => new(a.Amount * factor, a.Currency);
public static bool operator ==(Money a, Money b) => a.Amount == b.Amount && a.Currency == b.Currency;
public static bool operator !=(Money a, Money b) => !(a == b);
public static bool operator >(Money a, Money b)  { EnsureSameCurrency(a, b); return a.Amount > b.Amount; }
public static bool operator <(Money a, Money b)  { EnsureSameCurrency(a, b); return a.Amount < b.Amount; }
public static bool operator >=(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount >= b.Amount; }
public static bool operator <=(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount <= b.Amount; }
public static explicit operator decimal(Money m) => m.Amount;
```

**DLACZEGO:** `Money` to typ wartości (struct) hermetyzujący kwotę i walutę. Przeciążone operatory pozwalają pisać `balance >= amount` lub `interest + bonus` wprost, zamiast wywoływać metody pomocnicze — kod domenowy jest przez to czytelny i naturalny. Rzutowanie jawne na `decimal` wyraża intencję.

---

## 11. Programowanie asynchroniczne

**CO:** Programowanie asynchroniczne (async/await) pozwala wykonywać długotrwałe operacje (I/O, sieć, baza) bez blokowania wątku UI.

**JAK:**

- `src/BankApp.Core/Services/TransactionService.cs` — linia 14: `public async Task<Result<bool>> DepositAsync(...)`, linia 19: `await Task.Delay(150)`, linia 23: `await _accounts.SaveChangesAsync()`:

```csharp
// src/BankApp.Core/Services/TransactionService.cs, linie 14-25
public async Task<Result<bool>> DepositAsync(string accountNumber, Money amount)
{
    var account = await _accounts.GetByNumberAsync(accountNumber);
    if (account is null) return Result<bool>.Failure($"Nie znaleziono konta {accountNumber}.");
    await Task.Delay(150); // symulacja przetwarzania (async)
    // ...
    await _accounts.SaveChangesAsync();
    // ...
}
```

- `src/BankApp.Data/Repository.cs` — linia 11–14: `await Db.Set<T>().FindAsync(id)`, `await Db.Set<T>().ToListAsync()`, `await Db.SaveChangesAsync()`.
- `src/BankApp.Wpf/Mvvm/AsyncRelayCommand.cs` — linia 19: `public async void Execute(...)` — bezpieczne wykonanie asynchronicznej komendy WPF.
- `src/BankApp.Wpf/ViewModels/DiagnosticsViewModel.cs` — linia 31: `private async Task GenerateAsync()` — asynchroniczny eksport raportu CSV.

**DLACZEGO:** Operacje bazodanowe (EF Core) i sieciowe są naturalnie asynchroniczne. Komendy WPF muszą być async, aby nie zamrażać interfejsu podczas operacji bankowych. `AsyncRelayCommand` blokuje powtórne kliknięcie (`_isRunning`) przez czas trwania operacji.

---

## 12. Refleksja

**CO:** Refleksja pozwala programowi badać i używać informacji o typach, metodach i atrybutach w czasie wykonania.

**JAK:**

- `src/BankApp.Core/Reflection/AccountTypeRegistry.cs` — linie 13–26: skanowanie assembly i odczyt atrybutu:

```csharp
// src/BankApp.Core/Reflection/AccountTypeRegistry.cs, linie 14-25
var assembly = typeof(Account).Assembly;
foreach (var type in assembly.GetTypes())
{
    if (type.IsAbstract || !typeof(Account).IsAssignableFrom(type))
        continue;
    var info = type.GetCustomAttribute<AccountTypeInfoAttribute>();
    if (info is null) continue;
    yield return new AccountTypeDescriptor(info.Key, info.DisplayName, info.Description, type);
}
```

- `src/BankApp.Core/Reflection/ReportGenerator.cs` — linia 15: `typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)`, linia 20: `p.GetCustomAttribute<ReportColumnAttribute>()?.Header ?? p.Name`:

```csharp
// src/BankApp.Core/Reflection/ReportGenerator.cs, linie 15-20
var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(p => p.CanRead && IsSimple(p.PropertyType))
    .ToArray();
var headers = props.Select(p =>
    p.GetCustomAttribute<ReportColumnAttribute>()?.Header ?? p.Name);
```

**DLACZEGO:** `AccountTypeRegistry.Discover()` automatycznie wykrywa wszystkie podklasy `Account` oznaczone atrybutem `[AccountTypeInfo]` — dodanie nowego typu konta nie wymaga modyfikacji żadnej listy w GUI ani fabryce. `ReportGenerator` jest w pełni generyczny: obsługuje dowolny typ `T`, odczytując jego właściwości i opcjonalne nagłówki z atrybutu `[ReportColumn]` w czasie wykonania.

---

## Dodatki

### Wzorce projektowe

- **Repository** (`src/BankApp.Data/Repository.cs`, `AccountRepository.cs`) — oddziela logikę dostępu do danych od domeny.
- **MVVM** (`src/BankApp.Wpf/Mvvm/ViewModelBase.cs`, `MainViewModel.cs`, `DiagnosticsViewModel.cs`) — separacja widoku, logiki prezentacji i modelu domenowego.
- **Strategy** (`src/BankApp.Core/Interest/IInterestStrategy.cs`, `InterestStrategies.cs`) — algorytm oprocentowania jest wymienialny bez zmiany konta.
- **Factory** (`src/BankApp.Core/Accounts/AccountFactory.cs`) — tworzy konta po kluczu string (switch expression).
- **Observer** (`ITransactionService.TransactionCompleted` + subskrypcja w `MainViewModel`) — ViewModel jest powiadamiany o zdarzeniach serwisowych bez ścisłego sprzężenia.
- **Dependency Injection** (`src/BankApp.Wpf/App.xaml.cs`) — serwisy rejestrowane w `Microsoft.Extensions.DependencyInjection`, wstrzykiwane przez konstruktory.

### Własne wyjątki

`src/BankApp.Core/Exceptions.cs` — hierarchia: `BankException : Exception` (klasa bazowa), `InsufficientFundsException : BankException`, `AccountNotFoundException : BankException`. Pozwala złapać wszystkie wyjątki domenowe jednym `catch (BankException)`.

### Result\<T\>

`src/BankApp.Core/Result.cs` — niezmienny struct generyczny zwracający sukces z wartością lub błąd z komunikatem. Eliminuje wyjątki w normalnym przepływie sterowania (np. nieudana wpłata to nie wyjątek, lecz `Result<bool>.Failure(...)`).

### Atrybuty niestandardowe

- `src/BankApp.Core/Attributes/AccountTypeInfoAttribute.cs` — atrybut `[AccountTypeInfo(key, displayName, description)]` stosowany do klas kont, odczytywany przez `AccountTypeRegistry`.
- `src/BankApp.Core/Attributes/ReportColumnAttribute.cs` — atrybut `[ReportColumn(header)]` stosowany do właściwości, odczytywany przez `ReportGenerator` jako nagłówek kolumny CSV.

### LINQ, switch expression, pattern matching, rekordy, nullable

- **LINQ** — używany m.in. w `ReportGenerator.cs` (`.Where`, `.Select`, `.ToArray`), `AccountTypeRegistry.cs`, `MainViewModel.cs` (`Accounts.FirstOrDefault()`).
- **Switch expression** — `src/BankApp.Core/Accounts/AccountFactory.cs` linia 9: `typeKey switch { "Checking" => ..., "Savings" => ..., ... }`.
- **Pattern matching** — `src/BankApp.Core/Transaction.cs` linia 16: `Type switch { TransactionType.Withdrawal or TransactionType.TransferOut or TransactionType.Fee => -Amount, _ => Amount }`.
- **Rekordy** — `src/BankApp.Core/Services/TransactionEventArgs.cs` linia 4: `public record TransactionEventArgs(...)` — niezmienny DTO zdarzenia; `src/BankApp.Core/Reflection/AccountTypeRegistry.cs` linia 7: `public sealed record AccountTypeDescriptor(...)`.
- **Nullable reference types** — włączone w całym rozwiązaniu (`<Nullable>enable</Nullable>`); widoczne m.in. w `Account.Customer?`, `Result<T>.Error?`, `EventHandler<...>?`.

### EF Core TPH + SQLite

`src/BankApp.Data/BankDbContext.cs` — wszystkie typy kont (`CheckingAccount`, `SavingsAccount`, `CreditAccount`) mapowane do jednej tabeli `Accounts` (Table-Per-Hierarchy) z kolumną dyskryminatora `AccountType` (linia 49–52).

Kolumna `BalanceAmount` przechowywana jako `TEXT` w SQLite (linia 56: `.HasColumnType("TEXT")`), ponieważ SQLite nie posiada natywnego typu `DECIMAL` — dzięki temu nie ma utraty precyzji dla liczb dziesiętnych.

`Account.CustomerId` (int, nie int?) jest **ignorowane przez EF** (linia 80: `.Ignore(a => a.CustomerId)`) i zastąpione shadow property `CustomerFk` (int?) (linia 81), co pozwala mieć w bazie NULL-owalną kolumnę FK (konto może nie mieć klienta). Po odczycie z bazy `CustomerFkSyncInterceptor` (linia 15) implementuje `IMaterializationInterceptor` i synchronizuje domenowe `CustomerId` z wartością shadow FK, aby kod domenowy widział poprawne ID właściciela konta.

### Testy jednostkowe

`tests/BankApp.Tests` — 40 testów xUnit, 0 błędów. Obszary:

| Plik | Zakres |
|---|---|
| `MoneyTests.cs` | Operatory arytmetyczne i porównawcze `Money`, rzutowanie, różne waluty |
| `TransactionTests.cs` | `SignedAmount` dla każdego `TransactionType`, konstruktor |
| `ResultTests.cs` | `Result<T>.Success` / `Failure`, pola `IsSuccess`, `Value`, `Error` |
| `AccountBehaviorTests.cs` | Deposit/Withdraw/Post, `InsufficientFundsException`, polimorfizm opłat |
| `IndexerTests.cs` | Indeksatory `Customer[int]` i `Bank[string]`, `AccountNotFoundException` |
| `InterestStrategyTests.cs` | `StandardInterestStrategy`, `PromotionalInterestStrategy`, `DelegateInterestStrategy` |
| `StaticMembersTests.cs` | Unikalność `AccountNumberGenerator.Next()`, `CurrencyFormatter.Format`, `AccountFactory.Create` |
| `ReflectionTests.cs` | `AccountTypeRegistry.Discover` (liczba i zawartość), `ReportGenerator.Generate` (nagłówki CSV) |
| `TransactionServiceTests.cs` | `DepositAsync`/`WithdrawAsync`/`TransferAsync` z `FakeAccountRepository`, zdarzenie `TransactionCompleted` |
| `RepositoryTests.cs` | `Repository<T>` + EF Core in-memory (InMemory provider) |
| `SqliteIntegrationTests.cs` | Pełny round-trip SQLite: zapis i odczyt konta z transakcjami |
| `SeedIntegrationTests.cs` | `DbInitializer` — seed danych, weryfikacja klientów i typów kont |
