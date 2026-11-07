using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using Microsoft.Extensions.Logging;

namespace HassanTraders.Desktop.Features.Items;

/// <summary>One entry of a category or type filter. <see cref="Id"/> null means "all".</summary>
public sealed record FilterOption(long? Id, string Label);

/// <summary>
/// Items list (CLAUDE.md F1, F5, F24): real catalogue data, search by Urdu/English name, brand or barcode,
/// then filter by category and type. Everything is filtered in memory, so 10,000 items stay instant.
/// </summary>
public sealed partial class ItemsViewModel : LocalizedViewModel
{
    private static readonly char[] Separators = [' ', '\t'];

    private readonly IUseCaseRunner _runner;
    private readonly ILogger<ItemsViewModel> _logger;
    private readonly CatalogFormatter _formatter;
    private readonly CancellationTokenSource _lifetime = new();
    private List<ItemRowViewModel> _all = [];
    private CatalogListing? _listing;
    private bool _suppressFilter;

    public ItemsViewModel(IUseCaseRunner runner, ILocalizationService localization, ILogger<ItemsViewModel> logger)
        : base(localization)
    {
        _runner = runner;
        _logger = logger;
        _formatter = new CatalogFormatter(localization);
        _categoryOptions = [new FilterOption(null, localization["Items_AllCategories"])];
        _typeOptions = [new FilterOption(null, localization["Items_AllTypes"])];
        _selectedCategory = _categoryOptions[0];
        _selectedType = _typeOptions[0];
        LoadTask = LoadAsync();
    }

    /// <summary>Completes when the first load has finished (used by tests).</summary>
    public Task LoadTask { get; private set; }

    public BulkObservableCollection<ItemRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowGrid), nameof(ShowLoading), nameof(ShowError), nameof(ShowEmpty), nameof(ShowNoMatches))]
    private ViewState _state = ViewState.Loading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilter))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<FilterOption> _categoryOptions;

    [ObservableProperty]
    private IReadOnlyList<FilterOption> _typeOptions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilter), nameof(HasTypes))]
    private FilterOption? _selectedCategory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilter))]
    private FilterOption? _selectedType;

    public bool ShowGrid => State == ViewState.Ready;

    public bool ShowLoading => State == ViewState.Loading;

    public bool ShowError => State == ViewState.Error;

    public bool ShowEmpty => State == ViewState.Empty;

    public bool ShowNoMatches => State == ViewState.NoMatches;

    public bool HasTypes => TypeOptions.Count > 1;

    public bool HasActiveFilter =>
        !string.IsNullOrWhiteSpace(SearchText) || SelectedCategory?.Id is not null || SelectedType?.Id is not null;

    public string Title => Localization["Items_Title"];

    public string Subtitle => State is ViewState.Ready or ViewState.NoMatches
        ? Localization.Format("Items_Showing", Rows.Count, _all.Count)
        : string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedCategoryChanged(FilterOption? value)
    {
        if (_suppressFilter)
        {
            return;
        }

        RebuildTypeOptions(selectedTypeId: null);
        ApplyFilter();
    }

    partial void OnSelectedTypeChanged(FilterOption? value) => ApplyFilter();

    [RelayCommand]
    private void ClearFilters()
    {
        _suppressFilter = true;
        SearchText = string.Empty;
        SelectedCategory = CategoryOptions[0];
        _suppressFilter = false;
        RebuildTypeOptions(selectedTypeId: null);
        ApplyFilter();
    }

    [RelayCommand]
    private Task RetryAsync()
    {
        LoadTask = LoadAsync();
        return LoadTask;
    }

    protected override void OnLanguageChanged()
    {
        if (_listing is null)
        {
            return;
        }

        var categoryId = SelectedCategory?.Id;
        var typeId = SelectedType?.Id;
        foreach (var row in _all)
        {
            row.ApplyLanguage(_formatter, Localization);
        }

        _suppressFilter = true;
        BuildCategoryOptions(categoryId);
        _suppressFilter = false;
        RebuildTypeOptions(typeId);
        OnPropertyChanged(nameof(Subtitle));
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
            var listing = await _runner.RunAsync<ICatalogQueryService, CatalogListing>(s => s.GetListingAsync(token));
            token.ThrowIfCancellationRequested();
            _listing = listing;
            _all = listing.Items.Select(i => new ItemRowViewModel(i, _formatter, Localization)).ToList();
            _suppressFilter = true;
            BuildCategoryOptions(selectedId: null);
            _suppressFilter = false;
            RebuildTypeOptions(selectedTypeId: null);
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            // The page was closed while loading; nothing to show.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Loading the items list failed");
            State = ViewState.Error;
        }
    }

    private void BuildCategoryOptions(long? selectedId)
    {
        var useUrdu = Localization.Language == "ur";
        var options = new List<FilterOption> { new(null, Localization["Items_AllCategories"]) };
        options.AddRange((_listing?.Categories ?? []).Select(c => new FilterOption(c.Id, useUrdu ? c.UrduName : c.Name)));
        CategoryOptions = options;
        SelectedCategory = options.FirstOrDefault(o => o.Id == selectedId) ?? options[0];
    }

    private void RebuildTypeOptions(long? selectedTypeId)
    {
        var useUrdu = Localization.Language == "ur";
        var options = new List<FilterOption> { new(null, Localization["Items_AllTypes"]) };
        var category = _listing?.Categories.FirstOrDefault(c => c.Id == SelectedCategory?.Id);
        if (category is not null)
        {
            options.AddRange(category.Types.Select(t => new FilterOption(t.Id, useUrdu ? t.UrduName : t.Name)));
        }

        var wasSuppressed = _suppressFilter;
        _suppressFilter = true;
        TypeOptions = options;
        SelectedType = options.FirstOrDefault(o => o.Id == selectedTypeId) ?? options[0];
        _suppressFilter = wasSuppressed;
        OnPropertyChanged(nameof(HasTypes));
    }

    private void ApplyFilter()
    {
        if (_suppressFilter || _listing is null)
        {
            return;
        }

        var terms = SearchNormalizer.Normalize(SearchText).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        var categoryId = SelectedCategory?.Id;
        var typeId = SelectedType?.Id;

        var matches = _all.Where(row =>
            (categoryId is null || row.CategoryId == categoryId) &&
            (typeId is null || row.ProductTypeId == typeId) &&
            terms.All(term => row.SearchKey.Contains(term, StringComparison.Ordinal)));
        Rows.ReplaceAll(matches);

        State = _all.Count == 0 ? ViewState.Empty : Rows.Count == 0 ? ViewState.NoMatches : ViewState.Ready;
        OnPropertyChanged(nameof(Subtitle));
    }

}
