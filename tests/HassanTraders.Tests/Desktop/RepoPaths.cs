namespace HassanTraders.Desktop.Tests;

internal static class RepoPaths
{
    public static string Root { get; } = FindRoot();

    public static string Desktop => Path.Combine(Root, "src", "HassanTraders.Desktop");

    public static string Resources => Path.Combine(Desktop, "Resources");

    /// <summary>Every .xaml under the Desktop project, excluding build output.</summary>
    public static IEnumerable<string> AllXaml() =>
        Directory.EnumerateFiles(Desktop, "*.xaml", SearchOption.AllDirectories)
            .Where(f =>
            {
                var relative = Path.GetRelativePath(Desktop, f);
                return !relative.StartsWith("obj", StringComparison.Ordinal) && !relative.StartsWith("bin", StringComparison.Ordinal);
            });

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "HassanTraders.Desktop", "Resources")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
