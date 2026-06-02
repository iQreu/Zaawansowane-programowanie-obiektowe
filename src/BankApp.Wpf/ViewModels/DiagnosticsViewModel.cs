using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using BankApp.Core.Reflection;
using BankApp.Core.Repositories;
using BankApp.Wpf.Mvvm;

namespace BankApp.Wpf.ViewModels;

public sealed class DiagnosticsViewModel : ViewModelBase
{
    private readonly IAccountRepository _accounts;

    public ObservableCollection<AccountTypeDescriptor> DiscoveredTypes { get; } = new();

    private string _report = string.Empty;
    public string Report { get => _report; set => SetField(ref _report, value); }

    public AsyncRelayCommand GenerateReportCommand { get; }

    public DiagnosticsViewModel(IAccountRepository accounts)
    {
        _accounts = accounts;

        foreach (var t in AccountTypeRegistry.Discover())
            DiscoveredTypes.Add(t);

        GenerateReportCommand = new AsyncRelayCommand(GenerateAsync);
    }

    private async Task GenerateAsync()
    {
        var all = await _accounts.GetAllAsync();
        var csv = ReportGenerator.Generate(all);
        Report = csv;

        var path = Path.Combine(AppContext.BaseDirectory, "raport_kont.csv");
        await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
        Report += $"\nZapisano do: {path}";
    }
}
