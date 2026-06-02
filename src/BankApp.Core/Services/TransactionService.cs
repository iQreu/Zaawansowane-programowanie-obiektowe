using BankApp.Core.Accounts;
using BankApp.Core.Repositories;

namespace BankApp.Core.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accounts;

    public event EventHandler<TransactionEventArgs>? TransactionCompleted;

    public TransactionService(IAccountRepository accounts) => _accounts = accounts;

    public async Task<Result<bool>> DepositAsync(string accountNumber, Money amount)
    {
        var account = await _accounts.GetByNumberAsync(accountNumber);
        if (account is null) return Result<bool>.Failure($"Nie znaleziono konta {accountNumber}.");

        await Task.Delay(150); // symulacja przetwarzania (async)
        try { account.Deposit(amount); }
        catch (Exception ex) { return Result<bool>.Failure(ex.Message); }

        await _accounts.SaveChangesAsync();
        Raise(account, TransactionType.Deposit, amount);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> WithdrawAsync(string accountNumber, Money amount)
    {
        var account = await _accounts.GetByNumberAsync(accountNumber);
        if (account is null) return Result<bool>.Failure($"Nie znaleziono konta {accountNumber}.");

        await Task.Delay(150);
        try { account.Withdraw(amount); }
        catch (Exception ex) { return Result<bool>.Failure(ex.Message); }

        await _accounts.SaveChangesAsync();
        Raise(account, TransactionType.Withdrawal, amount);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> TransferAsync(string fromNumber, string toNumber, Money amount)
    {
        var from = await _accounts.GetByNumberAsync(fromNumber);
        var to   = await _accounts.GetByNumberAsync(toNumber);
        if (from is null) return Result<bool>.Failure($"Nie znaleziono konta {fromNumber}.");
        if (to is null)   return Result<bool>.Failure($"Nie znaleziono konta {toNumber}.");

        await Task.Delay(200);
        try
        {
            var outTx = new Transaction(TransactionType.TransferOut, amount, $"Przelew do {toNumber}");
            if (!from.CanWithdraw(amount))
                throw new InsufficientFundsException(from.Number, from.Balance, amount);
            from.Post(outTx);
            to.Post(new Transaction(TransactionType.TransferIn, amount, $"Przelew z {fromNumber}"));
        }
        catch (Exception ex) { return Result<bool>.Failure(ex.Message); }

        await _accounts.SaveChangesAsync();
        Raise(from, TransactionType.TransferOut, amount);
        Raise(to, TransactionType.TransferIn, amount);
        return Result<bool>.Success(true);
    }

    private void Raise(Account account, TransactionType type, Money amount) =>
        TransactionCompleted?.Invoke(this, new TransactionEventArgs(account.Number, type, amount, account.Balance));
}
