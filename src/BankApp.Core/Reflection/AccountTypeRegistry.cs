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
