using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;
using System.Linq;
using System;

public static class ArchiveService
{
    public static void ExtractToDirectory(string inputPath, string outputPath)
    {
        Directory.CreateDirectory(outputPath);

        var ext = Path.GetExtension(inputPath).ToLowerInvariant();
        if (ext is not (".zip" or ".rar" or ".7z"))
            throw new InvalidOperationException("Unsupported archive.");

        using var archive = ArchiveFactory.OpenArchive(inputPath);

        var rootFull = Path.GetFullPath(outputPath);
        if (!rootFull.EndsWith(Path.DirectorySeparatorChar))
            rootFull += Path.DirectorySeparatorChar;

        foreach (var entry in archive.Entries.Where(x => !x.IsDirectory))
        {
            if (string.IsNullOrEmpty(entry.Key))
                continue;

            var outPath = Path.GetFullPath(Path.Combine(rootFull, entry.Key));
            if (!outPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                continue;

            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

            entry.WriteToFile(outPath, new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true
            });
        }
    }
}
