namespace HassanTraders.Desktop.Common.Controls;

/// <summary>Semantic colour of a pill or banner. Maps to the Success/Warning/Danger/Info tokens.</summary>
public enum Tone
{
    Neutral,
    Success,
    Warning,
    Danger,
    Info,
}

public enum StateKind
{
    Empty,
    Loading,
    Error,
}

public enum DialogKind
{
    Info,
    Error,
    Confirm,
}

/// <summary>Maps a <see cref="Tone"/> to design-system resource keys. Keys only, never colours.</summary>
internal static class ToneKeys
{
    public static string Foreground(Tone tone) => tone switch
    {
        Tone.Success => "Brush.Success",
        Tone.Warning => "Brush.Warning",
        Tone.Danger => "Brush.Danger",
        Tone.Info => "Brush.Info",
        _ => "Brush.TextSecondary",
    };

    public static string Background(Tone tone) => tone switch
    {
        Tone.Success => "Brush.SuccessBg",
        Tone.Warning => "Brush.WarningBg",
        Tone.Danger => "Brush.DangerBg",
        Tone.Info => "Brush.InfoBg",
        _ => "Brush.SurfaceHover",
    };
}
