using BankApp.Core.Accounts;
using BankApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Data;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(BankDbContext db) : base(db) { }

    public async Task<Account?> GetByNumberAsync(string number) =>
        await Db.Set<Account>()
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Number == number);
}
