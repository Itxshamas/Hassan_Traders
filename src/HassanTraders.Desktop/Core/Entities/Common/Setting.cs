namespace HassanTraders.Desktop.Core.Common;

/// <summary>A single key/value application setting (language, theme, printer, ...).</summary>
public sealed class Setting
{
    private Setting()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public Setting(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Key = key;
        Value = value ?? string.Empty;
    }

    public string Key { get; private set; }

    public string Value { get; private set; }

    public void Change(string value) => Value = value ?? string.Empty;
}
