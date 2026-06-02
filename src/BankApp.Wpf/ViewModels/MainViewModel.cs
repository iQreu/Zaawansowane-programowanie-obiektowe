using System.Collections.ObjectModel;
using System.Windows;
using BankApp.Core;
using BankApp.Core.Accounts;
using BankApp.Core.Reflection;
using BankApp.Core.Repositories;
using BankApp.Core.Services;
using BankApp.Wpf.Mvvm;

namespace BankApp.Wpf.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionService _service;

    public ObservableCollection<Account> Accounts { get; } = new();
    public ObservableCollection<AccountTypeDescriptor> AccountTypes { get; } = new();

    public DiagnosticsViewModel Diagnostics { get; }

    private Account? _selectedAccount;
    public Account? SelectedAccount
    {
        get => _selectedAccount;
        set { if (SetField(ref _selectedAccount, value)) OnPropertyChanged(nameof(Transactions)); }
    }

    public IEnumerable<Transaction> Transactions => _selectedAccount?.Transactions ?? Enumerable.Empty<Transaction>();

    private decimal _amount;
    public decimal Amount { get => _amount; set => SetField(ref _amount, value); }

    private string _targetNumber = string.Empty;
    public string TargetNumber { get => _targetNumber; set => SetField(ref _targetNumber, value); }

    private AccountTypeDescriptor? _selectedType;
    public AccountTypeDescriptor? SelectedType { get => _selectedType; set => SetField(ref _selectedType, value); }

    private string _status = "Gotowe.";
    public string Status { get => _status; set => SetField(ref _status, value); }

    public AsyncRelayCommand DepositCommand { get; }
    public AsyncRelayCommand WithdrawCommand { get; }
    public AsyncRelayCommand TransferCommand { get; }
    public AsyncRelayCommand OpenAccountCommand { get; }

    public MainViewModel(IAccountRepository accounts, ITransactionService service, DiagnosticsViewModel diagnostics)
    {
        _accounts = accounts;
        _service = service;
        Diagnostics = diagnostics;

        _service.TransactionCompleted += OnTransactionCompleted;

        DepositCommand = new AsyncRelayCommand(DepositAsync, () => SelectedAccount is not null && Amount > 0);
        WithdrawCommand = new AsyncRelayCommand(WithdrawAsync, () => SelectedAccount is not null && Amount > 0);
        TransferCommand = new AsyncRelayCommand(TransferAsync,
            () => SelectedAccount is not null && Amount > 0 && !string.IsNullOrWhiteSpace(TargetNumber));
        OpenAccountCommand = new AsyncRelayCommand(OpenAccountAsync, () => SelectedType is not null);

        foreach (var t in AccountTypeRegistry.Discover())
            AccountTypes.Add(t);
    }

    public async Task LoadAsync()
    {
        var savedNumber = SelectedAccount?.Number;
        Accounts.Clear();
        foreach (var a in await _accounts.GetAllAsync())
            Accounts.Add(a);
        SelectedAccount = Accounts.FirstOrDefault(a => a.Number == savedNumber) ?? Accounts.FirstOrDefault();
    }

    private async Task DepositAsync()
    {
        var result = await _service.DepositAsync(SelectedAccount!.Number, new Money(Amount));
        Report(result.IsSuccess, result.Error ?? $"Wpłacono {Amount:N2}.");
        await LoadAsync();
    }

    private async Task WithdrawAsync()
    {
        var result = await _service.WithdrawAsync(SelectedAccount!.Number, new Money(Amount));
        Report(result.IsSuccess, result.Error ?? $"Wypłacono {Amount:N2}.");
        await LoadAsync();
    }

    private async Task TransferAsync()
    {
        var result = await _service.TransferAsync(SelectedAccount!.Number, TargetNumber, new Money(Amount));
        Report(result.IsSuccess, result.Error ?? $"Przelano {Amount:N2} na {TargetNumber}.");
        await LoadAsync();
    }

    private async Task OpenAccountAsync()
    {
        var account = AccountFactory.Create(SelectedType!.Key);
        await _accounts.AddAsync(account);
        await _accounts.SaveChangesAsync();
        Status = $"Otwarto konto {account.Number} ({SelectedType.DisplayName}).";
        await LoadAsync();
    }

    private void OnTransactionCompleted(object? sender, TransactionEventArgs e) =>
        Status = $"[zdarzenie] {e.Type} {e.Amount} na {e.AccountNumber}. Nowe saldo: {e.NewBalance}.";

    private void Report(bool success, string message)
    {
        Status = message;
        if (!success) MessageBox.Show(message, "Błąd operacji", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
