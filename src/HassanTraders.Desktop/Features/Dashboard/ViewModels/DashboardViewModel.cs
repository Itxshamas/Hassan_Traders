using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using Microsoft.Extensions.Logging;

namespace HassanTraders.Desktop.Features.Dashboard;

/// <summary>A labelled horizontal bar. <see cref="Max"/> is the largest value in the same chart.</summary>
public sealed record BarRow(string Label, int Count, int Max);

/// <summary>
/// Dashboard. Shows only numbers that exist today (the catalogue). Sales, stock alerts and udhaar cards show
/// honest empty states until those modules are built (CLAUDE.md phases 3 to 5).
/// </summary>
public sealed partial class DashboardViewModel : LocalizedViewModel
{
    private readonly IUseCaseRunner _runner;
    private readonly ILogger<DashboardViewModel> _logger;
    private readonly CancellationTokenSource _lifetime = new();
    private CatalogSummary? _summary;

    public DashboardViewModel(IUseCaseRunner runner, ILocalizationService localization, ILogger<DashboardViewModel> logger)
        : base(localization)
    {
        _runner = runner;
        _logger = logger;
        LoadTask = LoadAsync();
    }

    public Task LoadTask { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowContent), nameof(ShowLoading), nameof(ShowError))]
    private ViewState _state = ViewState.Loading;

    [ObservableProperty]
    private string _activeItemsText = string.Empty;

    [ObservableProperty]
    private string _activeItemsHint = string.Empty;

    [ObservableProperty]
    private string _categoriesText = string.Empty;

    [ObservableProperty]
    private string _typesText = string.Empty;

    [ObservableProperty]
    private string _variantsText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<BarRow> _categoryBars = [];

    [ObservableProperty]
    private string _rangeTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRange))]
    private IReadOnlyList<BarRow> _rangeBars = [];

    public bool ShowContent => State == ViewState.Ready;

    public bool ShowLoading => State == ViewState.Loading;

    public bool ShowError => State == ViewState.Error;

    public bool HasRange => RangeBars.Count > 0;

    public string Title => Localization["Nav_Dashboard"];

    public string Subtitle => Localization["Dashboard_Subtitle"];

    [RelayCommand]
    private Task RetryAsync()
    {
        LoadTask = LoadAsync();
        return LoadTask;
    }

    protected override void OnLanguageChanged()
    {
        if (_summary is not null)
        {
            Present(_summary);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
    }

    private async Task LoadAsync()
    {
        State = ViewState.Loading;
        try
        {
            var token = _lifetime.Token;
            var summary = await _runner.RunAsync<ICatalogQueryService, CatalogSummary>(s => s.GetSummaryAsync(token));
            token.ThrowIfCancellationRequested();
            _summary = summary;
            Present(summary);
            State = ViewState.Ready;
        }
        catch (OperationCanceledException)
        {
            // Page closed while loading.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Loading the dashboard failed");
            State = ViewState.Error;
        }
    }

    private void Present(CatalogSummary summary)
    {
        var urdu = Localization.Language == "ur";
        ActiveItemsText = summary.ActiveItems.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ActiveItemsHint = summary.InactiveItems > 0
            ? Localization.Format("Dashboard_ActiveItemsHint", summary.InactiveItems)
            : Localization["Dashboard_AllActive"];
        CategoriesText = summary.CategoryCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        TypesText = summary.TypeCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        VariantsText = summary.VariantCount.ToString(System.Globalization.CultureInfo.InvariantCulture);

        CategoryBars = ToBars(summary.Categories.Select(c => (urdu ? c.UrduName : c.Name, c.ItemCount)));

        // The category with the most types is the "range" (for example Rice: Premium, Superior, ...).
        var range = summary.Categories.Where(c => c.Types.Count > 0).MaxBy(c => c.Types.Count);
        if (range is null)
        {
            RangeTitle = string.Empty;
            RangeBars = [];
        }
        else
        {
            RangeTitle = Localization.Format("Dashboard_RangeTitle", urdu ? range.UrduName : range.Name);
            RangeBars = ToBars(range.Types.Select(t => (urdu ? t.UrduName : t.Name, t.ItemCount)));
        }
    }

    private static List<BarRow> ToBars(IEnumerable<(string Label, int Count)> rows)
    {
        var list = rows.ToList();
        var max = Math.Max(1, list.Count == 0 ? 1 : list.Max(r => r.Count));
        return list.Select(r => new BarRow(r.Label, r.Count, max)).ToList();
    }
}
