# BankApp — projekt zaliczeniowy (Zaawansowane Programowanie Obiektowe)

**Data:** 2026-06-02
**Technologia:** C# / .NET 9, WPF (MVVM), Entity Framework Core + SQLite, xUnit

## 1. Cel

Aplikacja desktopowa (WPF) modelująca system bankowy. Logika domenowa demonstruje
w sposób czytelny i celowy wszystkie 12 wymaganych mechanizmów obiektowych oraz
zestaw dodatkowych mechanizmów spoza listy. Backend (domena + dane) jest oddzielony
od warstwy GUI.

## 2. Architektura — Podejście A (rozdzielone projekty)

Solution `BankApp.sln` z czterema projektami; zależności idą w jedną stronę:

```
BankApp.Wpf  ──▶  BankApp.Data  ──▶  BankApp.Core
BankApp.Tests ──▶ BankApp.Core (+ Data)
```

- **`BankApp.Core`** (class library) — czysta domena. Zero zależności od WPF i EF.
  - Encje: `Account` (abstract) i podklasy, `Customer`, `Transaction`, `Bank`.
  - Typ wartości `Money` (struct) z przeciążonymi operatorami.
  - Interfejsy: `IRepository<T>`, `ITransactionService`, `IInterestBearing`, `IInterestStrategy`.
  - Serwisy domenowe, własne wyjątki, atrybuty niestandardowe, narzędzia refleksji.
- **`BankApp.Data`** (class library) — EF Core: `BankDbContext`, generyczne repozytorium,
  migracje SQLite, seed danych.
- **`BankApp.Wpf`** (WPF app) — Views (XAML) + ViewModels (MVVM), bootstrap DI.
- **`BankApp.Tests`** (xUnit) — testy jednostkowe domeny.

## 3. Model domeny

### Typ wartości `Money` (struct)
Niezmienny (kwota + waluta). Przeciążone operatory: `+ - *`, `== !=`, `< > <= >=`,
konwersje `implicit/explicit`. Główne, „uczciwe" miejsce na przeciążanie operatorów.

### Hierarchia kont
```
abstract class Account            (numer, saldo, właściciel, historia transakcji)
├─ CheckingAccount    (limit debetu)
├─ SavingsAccount     (oprocentowanie; IInterestBearing)
└─ CreditAccount      (limit kredytowy, oprocentowanie zadłużenia; IInterestBearing)
```
- `Account` abstrakcyjna; metody `abstract Money CalculateMonthlyFee()`,
  `virtual bool CanWithdraw(Money amount)`, `override ToString()`.
- Konstruktory przeciążone + łańcuchowanie `: this(...)` / `: base(...)`.

### Pozostałe encje
- `Customer` — dane klienta + jego konta; **indeksator** `customer[i]` → i-te konto.
- `Transaction` — typ (`enum`), kwota (`Money`), data, opis.
- `Bank` — agreguje klientów/konta; **indeksator** `bank["nr_konta"]` → konto.

### Serwisy i abstrakcje
- `IRepository<T>` + `Repository<T>` (generyczne, EF).
- `ITransactionService` — wpłaty/wypłaty/przelewy (async), emituje zdarzenia.
- `IInterestStrategy` — wzorzec Strategy dla naliczania odsetek.
- Wyjątki: `InsufficientFundsException`, `AccountNotFoundException`.

## 4. Mapowanie wymaganych mechanizmów

| # | Mechanizm | Miejsce |
|---|-----------|---------|
| 1 | Klasy | `Account`, `Customer`, `Transaction`, `Bank`, serwisy, ViewModele |
| 2 | Konstruktory | Przeciążone + `: this(...)` / `: base(...)` |
| 3 | Właściwości / indeksatory | Auto/`init`/wyliczane; `bank["nr"]`, `customer[i]` |
| 4 | Statyczne | Generator numerów kont, `CurrencyFormatter`, fabryka `Account.Create(...)` |
| 5 | Dziedziczenie | `Account` → 3 podklasy |
| 6 | Polimorfizm | `virtual`/`override`: `CalculateMonthlyFee`, `CanWithdraw`, `ToString` |
| 7 | Interfejsy / Abstrakcje | `abstract Account`; `IRepository<T>`, `ITransactionService`, `IInterestBearing`, `IInterestStrategy` |
| 8 | Typy ogólne / Kolekcje | `Repository<T>`, `Result<T>`, `List<>`, `Dictionary<>`, `ObservableCollection<>` |
| 9 | Delegacje / Zdarzenia | `event EventHandler<TransactionEventArgs>`; `Action`/`Func`/`Predicate`; `RelayCommand` (ICommand), `INotifyPropertyChanged` |
| 10 | Przeciążanie operatorów | Struct `Money` |
| 11 | Asynchroniczność | `async/await` na EF (`ToListAsync`, `SaveChangesAsync`), async komendy, symulacja przetwarzania (`Task.Delay`) |
| 12 | Refleksja | Skan assembly → podtypy `Account` (lista typów kont); generyczny eksport raportu po właściwościach + atrybuty niestandardowe |

## 5. Dodatki spoza listy

- Wzorce: Repository, MVVM, Strategy, Factory, Observer, DI.
- Dependency Injection (`Microsoft.Extensions.DependencyInjection`).
- Własne wyjątki + spójna obsługa błędów (`Result<T>`).
- LINQ, pattern matching / `switch expression`, extension methods, rekordy,
  atrybuty niestandardowe, nullable reference types.
- Testy jednostkowe (xUnit).

## 6. Interfejs (WPF / MVVM)

Ekrany:
1. Lista klientów i ich kont (binding do `ObservableCollection`).
2. Szczegóły konta + historia transakcji (DataGrid).
3. Operacje: wpłata / wypłata / przelew (formularz z walidacją).
4. Otwórz nowe konto — lista typów budowana przez refleksję.
5. Diagnostyka / Raport — pokaz refleksji + eksport raportu do pliku.

Wzorzec MVVM: każdy ekran ma `View` (XAML) + `ViewModel`
(`INotifyPropertyChanged`, `RelayCommand`). ViewModele otrzymują serwisy przez DI.
Widoki nie znają EF ani domeny bezpośrednio.

## 7. Przepływ danych (przykład: przelew)

```
View (klik) → RelayCommand → ViewModel.TransferAsync()
  → ITransactionService.TransferAsync()  (async)
      → walidacja (CanWithdraw, Money) → modyfikacja kont
      → Repository.SaveChangesAsync()  (EF/SQLite)
      → emisja zdarzenia TransactionCompleted
  → ViewModel aktualizuje ObservableCollection → GUI odświeża się przez binding
```

## 8. Obsługa błędów

- Domena rzuca własne wyjątki.
- Serwis zwraca `Result<T>` lub łapie wyjątki na granicy.
- ViewModel zamienia błędy na komunikaty GUI (MessageBox / pasek statusu).
- GUI nie wywala się niezłapanym wyjątkiem.

## 9. Baza danych

- EF Core + SQLite, `BankDbContext` z `DbSet`ami.
- Dziedziczenie kont mapowane jako **TPH** (Table-Per-Hierarchy, kolumna dyskryminatora).
- Migracje EF; przy starcie utworzenie/migracja bazy + seed przykładowych danych.

## 10. Testy (xUnit)

- Operatory `Money`.
- Naliczanie odsetek (polimorfizm wg typu konta).
- Reguły `CanWithdraw` (debet, limit kredytowy).
- Logika przelewu (repozytorium in-memory / EF InMemory).

## 11. Dokument wyjaśniający (`DOKUMENTACJA.md`)

Prowadzony równolegle z implementacją dokument w korzeniu repozytorium. Dla
każdego z 12 wymaganych mechanizmów (oraz dodatków) opisuje:
- **Co** — który mechanizm i jego krótkie wyjaśnienie.
- **Jak** — konkretne miejsce w kodzie (`plik:linia`) + krótki fragment.
- **Dlaczego** — uzasadnienie, dlaczego użyto go akurat w tym miejscu
  (a nie sztucznie).

Aktualizowany po każdym etapie planu, tak by na końcu stanowił kompletną
ściągę do obrony/oddania projektu.
