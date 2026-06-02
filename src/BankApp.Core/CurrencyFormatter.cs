using System.Globalization;

namespace BankApp.Core;

public static class CurrencyFormatter
{
    private static readonly CultureInfo Pl = CultureInfo.GetCultureInfo("pl-PL");
    public static string Format(Money money) => $"{money.Amount.ToString("N2", Pl)} {money.Currency}";
}
