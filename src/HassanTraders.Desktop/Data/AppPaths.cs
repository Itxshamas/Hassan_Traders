using System.IO;

namespace HassanTraders.Desktop.Data;

public static class AppPaths
{
    public const string AppFolderName = "HassanTraders";
    public const string DatabaseFileName = "hassantraders.db";

    /// <summary>%LocalAppData%\HassanTraders on Windows.</summary>
    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    public static string DefaultDatabasePath { get; } = Path.Combine(DataDirectory, DatabaseFileName);

    public static string LogDirectory { get; } = Path.Combine(DataDirectory, "logs");
}
