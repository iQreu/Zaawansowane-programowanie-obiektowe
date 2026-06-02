# BankApp — Zaawansowane programowanie obiektowe

**Autor: Robert 82570**

## Streszczony opis

BankApp to desktopowa aplikacja bankowa napisana w C# / .NET 9 (WPF, wzorzec MVVM)
z trwałością danych w bazie SQLite poprzez Entity Framework Core. Modeluje klientów,
różne typy kont (osobiste, oszczędnościowe, kredytowe), wpłaty, wypłaty i przelewy
wraz z historią transakcji, naliczaniem odsetek i prostą diagnostyką/raportem.

Projekt powstał jako zaliczenie przedmiotu „Zaawansowane programowanie obiektowe"
i celowo demonstruje 12 wymaganych mechanizmów obiektowych (klasy, konstruktory,
właściwości/indeksatory, składowe statyczne, dziedziczenie, polimorfizm,
interfejsy/abstrakcje, typy ogólne/kolekcje, delegacje/zdarzenia, przeciążanie
operatorów, programowanie asynchroniczne, refleksję) oraz dodatki: wstrzykiwanie
zależności, własne wyjątki, `Result<T>`, atrybuty niestandardowe, EF Core TPH
i testy jednostkowe (xUnit).

Szczegółowe mapowanie każdego mechanizmu na konkretne miejsce w kodzie znajduje się
w pliku [`DOKUMENTACJA.md`](DOKUMENTACJA.md).

## Architektura

- **BankApp.Core** — domena (encje, `Money`, serwisy, interfejsy, refleksja); bez zależności od WPF/EF.
- **BankApp.Data** — EF Core + SQLite (`DbContext`, repozytoria, migracje, seed).
- **BankApp.Wpf** — interfejs (WPF/MVVM, jasny motyw fintech).
- **BankApp.Tests** — testy jednostkowe i integracyjne (xUnit).

## Jak uruchomić

```bash
dotnet build                          # kompilacja całości
dotnet test                           # uruchomienie testów
dotnet run --project src/BankApp.Wpf  # uruchomienie aplikacji
```
