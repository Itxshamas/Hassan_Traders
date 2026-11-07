using FluentValidation;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Services.Application.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HassanTraders.Desktop.Services.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>(ServiceLifetime.Singleton);
        services.AddSingleton<IUserSession, UserSession>();
        services.AddSingleton<IUseCaseRunner, UseCaseRunner>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppSettingsService, AppSettingsService>();
        services.AddScoped<IItemCatalogImportService, ItemCatalogImportService>();
        services.AddScoped<ICatalogQueryService, CatalogQueryService>();
        return services;
    }
}
