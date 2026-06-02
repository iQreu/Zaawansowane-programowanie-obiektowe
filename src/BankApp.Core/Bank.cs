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
