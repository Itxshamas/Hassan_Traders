using System.Text;

namespace HassanTraders.Desktop.Services;

/// <summary>
/// Makes search forgiving for Urdu and English: ignores case, diacritics and the Arabic/Urdu letter variants
/// that different keyboards produce for the same letter (ي/ی, ك/ک, ه/ہ).
/// </summary>
public static class SearchNormalizer
{
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var ch in text.Trim().ToLowerInvariant())
        {
            switch (ch)
            {
                case >= 'ً' and <= 'ٟ': // Arabic diacritics (zabar, zer, pesh, ...)
                case 'ٰ': // superscript alef
                case 'ـ': // tatweel
                    continue;
                case 'ي' or 'ى' or 'ے': // Arabic yeh, alef maksura, yeh barree -> Urdu yeh
                    builder.Append('ی');
                    break;
                case 'ك': // Arabic kaf -> Urdu kaf
                    builder.Append('ک');
                    break;
                case 'ه' or 'ة': // Arabic heh, teh marbuta -> Urdu heh goal
                    builder.Append('ہ');
                    break;
                default:
                    builder.Append(ch);
                    break;
            }
        }

        return builder.ToString();
    }
}
