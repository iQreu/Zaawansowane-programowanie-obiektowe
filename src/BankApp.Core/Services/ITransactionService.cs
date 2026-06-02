namespace BankApp.Core.Services;

public interface ITransactionService
{
    event EventHandler<TransactionEventArgs>? TransactionCompleted;

    Task<Result<bool>> DepositAsync(string accountNumber, Money amount);
    Task<Result<bool>> WithdrawAsync(string accountNumber, Money amount);
    Task<Result<bool>> TransferAsync(string fromNumber, string toNumber, Money amount);
}
