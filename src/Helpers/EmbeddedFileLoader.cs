namespace ChestSnap.Helpers;

public static class EmbeddedFileLoader
{
    public static Stream? GetStream(string fileName)
    {
        var assembles = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assembles)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();
            var firstOrDefault = resourceNames.FirstOrDefault(x => x.ToLower().EndsWith(fileName.ToLower()));
            if (firstOrDefault is null) continue;

            var resourceStream = assembly.GetManifestResourceStream(firstOrDefault);
            if (resourceStream is null)
            {
                Log.Warning($"Не удалось получить поток для ресурса: {fileName}");
                return null;
            }

            return resourceStream;
        }

        Log.Warning($"Ресурс '{fileName}' не найден в манифесте");
        return null;
    }

    public static List<string> GetResources()
    {
        List<string> result = [];
        var assembles = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assembles)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();
            result.AddRange(resourceNames);
        }

        return result;
    }

    public static string? GetText(string fileName)
    {
        using var resourceStream = GetStream(fileName);
        if (resourceStream is null) return null;

        try
        {
            using var streamReader = new StreamReader(resourceStream);
            return streamReader.ReadToEnd();
        }
        catch (Exception ex)
            { Log.Error(ex, $"Failed to read from stream {fileName}"); }

        return null;
    }
}