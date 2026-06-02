using BankApp.Core.Accounts;

namespace BankApp.Core.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByNumberAsync(string number);
}
