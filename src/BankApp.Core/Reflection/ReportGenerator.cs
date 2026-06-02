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
