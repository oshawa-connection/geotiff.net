namespace GeotiffTests;

public class GeoTiffTestBaseClass
{
    protected CancellationTokenSource cts = new();

    protected string GetDataFolderPath()
    {
        // Start from the directory where the test assembly is located
        string? dir = AppContext.BaseDirectory;

        // Walk up until we find the project root (i.e., contains .csproj or known marker)
        while (!Directory.GetFiles(dir, "*.csproj").Any())
        {
            dir = Directory.GetParent(dir)?.FullName
                  ?? throw new Exception("Could not locate project root.");
        }

        // Now construct path to Data folder
        string? dataPath = Path.Combine(dir, "Data");

        if (!Directory.Exists(dataPath))
        {
            throw new DirectoryNotFoundException($"Data folder not found at {dataPath}");
        }

        return dataPath;
    }
}