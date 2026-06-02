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
