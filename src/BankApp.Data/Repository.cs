using BankApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Data;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BankDbContext Db;
    public Repository(BankDbContext db) => Db = db;

    public async Task<T?> GetByIdAsync(int id) => await Db.Set<T>().FindAsync(id);
    public async Task<IReadOnlyList<T>> GetAllAsync() => await Db.Set<T>().ToListAsync();
    public async Task AddAsync(T entity) => await Db.Set<T>().AddAsync(entity);
    public async Task SaveChangesAsync() => await Db.SaveChangesAsync();
}
