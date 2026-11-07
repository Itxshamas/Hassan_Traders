using System.IO;
using System.Windows;
using System.Windows.Threading;
using HassanTraders.Desktop.Services.Application;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Features.Dashboard;
using HassanTraders.Desktop.Features.Items;
using HassanTraders.Desktop.Features.Login;
using HassanTraders.Desktop.Features.Settings;
using HassanTraders.Desktop.Common.Converters;
using HassanTraders.Desktop.Common.Helpers;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Shell;
using HassanTraders.Desktop.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HassanTraders.Desktop;

public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = @"Local\HassanTraders.SingleInstance";

    private IHost? _host;
    // Static: held for the whole process lifetime so a second copy can detect this one.
    private static Mutex? _singleInstance;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Handlers first, so nothing after this point can crash the app silently.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _singleInstance = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            var loc = LocalizationService.Instance;
            MessageBox.Show(loc["Common_AlreadyRunning"], loc["App_Title"], MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        try
        {
            // WPF-UI controls take their font from this theme resource instead of inheriting it, so keep it in step with the language.
            ApplyFontResource();
            LocalizationService.Instance.PropertyChanged += (_, _) => ApplyFontResource();

            _host = BuildHost(e.Args);
            await _host.StartAsync();

            await _host.Services.GetRequiredService<IUseCaseRunner>()
                .RunAsync<IDatabaseInitializer>(i => i.InitializeAsync());
            await _host.Services.GetRequiredService<IAppearanceService>().InitializeAsync();

            var root = _host.Services.GetRequiredService<RootViewModel>();
            root.Start();
            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Startup failed");
            var loc = LocalizationService.Instance;
            MessageBox.Show(loc["Common_StartupError"], loc["Common_StartupErrorTitle"], MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }

        _singleInstance?.Dispose();
        await Log.CloseAndFlushAsync();
        base.OnExit(e);
    }

    private static IHost BuildHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        });

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(AppPaths.LogDirectory, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: false)
            .CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger, dispose: false);

        var services = builder.Services;
        services.AddApplication();
        services.AddInfrastructure(builder.Configuration);

        services.AddSingleton<ILocalizationService>(LocalizationService.Instance);
        services.AddSingleton<IThemeApplier, WpfThemeApplier>();
        services.AddSingleton<IAppearanceService, AppearanceService>();
        services.AddSingleton<IDialogService, WpfDialogService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<RootViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ChangePasswordViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ItemsViewModel>();
        services.AddTransient<Func<LoginViewModel>>(sp => sp.GetRequiredService<LoginViewModel>);
        services.AddTransient<Func<ChangePasswordViewModel>>(sp => sp.GetRequiredService<ChangePasswordViewModel>);
        services.AddTransient<Func<ShellViewModel>>(sp => sp.GetRequiredService<ShellViewModel>);
        services.AddSingleton<MainWindow>();

        return builder.Build();
    }

    private void ApplyFontResource() =>
        Resources["ContentControlThemeFontFamily"] = BoolToFontFamilyConverter.For(LocalizationService.Instance.IsRightToLeft);

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportUnexpected(e.Exception, "UI thread");
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) =>
        ReportUnexpected(e.ExceptionObject as Exception, "AppDomain");

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }

    private void ReportUnexpected(Exception? exception, string source)
    {
        Log.Error(exception, "Unhandled exception on {Source}", source);
        var loc = LocalizationService.Instance;
        var message = loc["Common_UnexpectedError"];
        var title = loc["Common_ErrorTitle"];
        try
        {
            if (_host?.Services.GetService<IDialogService>() is { } dialogs)
            {
                dialogs.ShowError(title, message);
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (InvalidOperationException)
        {
            // Logging above is enough if no dialog can be shown (for example during shutdown).
        }
    }
}
