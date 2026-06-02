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
