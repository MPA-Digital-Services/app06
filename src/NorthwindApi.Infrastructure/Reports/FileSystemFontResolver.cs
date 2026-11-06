using PdfSharp.Fonts;

namespace NorthwindApi.Infrastructure.Reports;

/// <summary>
/// Resolutor de fuentes para plataformas que no son Windows (p. ej. contenedores Linux).
/// El build Core de PDFsharp 6 no carga fuentes automáticamente; este resolutor localiza una
/// familia TrueType instalada en el sistema (Liberation/DejaVu/FreeSans are metric-compatible
/// con Arial) y la usa como sustituto. Si no encuentra ninguna fuente, delega en
/// <see cref="PlatformFontResolver"/>.
/// </summary>
internal sealed class FileSystemFontResolver : IFontResolver
{
    private static readonly string[] FontRoots =
    {
        "/usr/share/fonts",
        "/usr/local/share/fonts",
        Path.Combine(AppContext.BaseDirectory, "Fonts")
    };

    // Orden de preferencia. Liberation es métricamente compatible con Arial/Times New Roman.
    private static readonly (string Regular, string Bold, string Italic, string BoldItalic)[] Candidates =
    {
        ("LiberationSans-Regular.ttf", "LiberationSans-Bold.ttf", "LiberationSans-Italic.ttf", "LiberationSans-BoldItalic.ttf"),
        ("DejaVuSans.ttf", "DejaVuSans-Bold.ttf", "DejaVuSans-Oblique.ttf", "DejaVuSans-BoldOblique.ttf"),
        ("FreeSans.ttf", "FreeSansBold.ttf", "FreeSansOblique.ttf", "FreeSansBoldOblique.ttf")
    };

    private readonly Dictionary<string, string> _faces = new(StringComparer.Ordinal);

    public FileSystemFontResolver()
    {
        foreach (var (regular, bold, italic, boldItalic) in Candidates)
        {
            var regularPath = Find(regular);
            if (regularPath is null)
                continue;

            _faces["regular"] = regularPath;
            _faces["bold"] = Find(bold) ?? regularPath;
            _faces["italic"] = Find(italic) ?? regularPath;
            _faces["bolditalic"] = Find(boldItalic) ?? Find(bold) ?? regularPath;
            break;
        }
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (_faces.Count == 0)
        {
            // Sin fuentes locales: delega en el resolutor de plataforma.
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        var face = (isBold, isItalic) switch
        {
            (true, true) => "bolditalic",
            (true, false) => "bold",
            (false, true) => "italic",
            _ => "regular"
        };

        return new FontResolverInfo(face, false, false);
    }

    public byte[]? GetFont(string faceName)
        => _faces.TryGetValue(faceName, out var path) ? File.ReadAllBytes(path) : null;

    private static string? Find(string fileName)
    {
        foreach (var root in FontRoots)
        {
            if (!Directory.Exists(root))
                continue;

            var match = Directory.EnumerateFiles(root, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (match is not null)
                return match;
        }

        return null;
    }
}
