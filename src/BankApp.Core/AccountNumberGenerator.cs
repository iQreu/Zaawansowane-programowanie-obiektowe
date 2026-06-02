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
