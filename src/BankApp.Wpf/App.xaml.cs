using System.IO;
using System.Windows;
using BankApp.Core.Repositories;
using BankApp.Core.Services;
using BankApp.Data;
using BankApp.Wpf.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Wpf;

public partial class App : Application
{
    private ServiceProvider _services = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dbPath = Path.Combine(AppContext.BaseDirectory, "bank.db");

        var collection = new ServiceCollection();
        collection.AddDbContext<BankDbContext>(o => o.UseSqlite($"Data Source={dbPath}"),
            ServiceLifetime.Singleton);
        collection.AddSingleton<IAccountRepository, AccountRepository>();
        collection.AddSingleton<ITransactionService, TransactionService>();
        collection.AddSingleton<DiagnosticsViewModel>();
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<MainWindow>();

        _services = collection.BuildServiceProvider();

        var db = _services.GetRequiredService<BankDbContext>();
        await DbInitializer.SeedAsync(db);

        var window = _services.GetRequiredService<MainWindow>();
        await _services.GetRequiredService<MainViewModel>().LoadAsync();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
