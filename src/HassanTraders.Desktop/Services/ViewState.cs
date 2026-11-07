namespace HassanTraders.Desktop.Services;

/// <summary>What a data page is showing. Each state has a matching design-system StateView or grid.</summary>
public enum ViewState
{
    Loading,
    Ready,
    Error,
    Empty,
    NoMatches,
}
