using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Tests;

public class SearchTests
{
    [Theory]
    [InlineData("  INAM Supreme ", "inam supreme")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    public void Normalize_IgnoresCaseAndOuterSpaces(string? input, string expected)
    {
        Assert.Equal(expected, SearchNormalizer.Normalize(input));
    }

    [Fact]
    public void Normalize_TreatsArabicAndUrduLetterVariantsAsTheSame()
    {
        // Arabic yeh/kaf/heh (typed on an Arabic keyboard) must find Urdu yeh/kaf/heh text.
        Assert.Equal(SearchNormalizer.Normalize("دال ماش كهلي"), SearchNormalizer.Normalize("دال ماش کہلی"));
        Assert.Equal(SearchNormalizer.Normalize("چيني"), SearchNormalizer.Normalize("چینی"));
    }

    [Fact]
    public void Normalize_IgnoresDiacritics()
    {
        Assert.Equal(SearchNormalizer.Normalize("انعام"), SearchNormalizer.Normalize("اِنعام"));
    }

    [Fact]
    public void BulkCollection_RaisesOneResetForAWholeReplacement()
    {
        var collection = new BulkObservableCollection<int>();
        var resets = 0;
        var other = 0;
        collection.CollectionChanged += (_, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                resets++;
            }
            else
            {
                other++;
            }
        };

        collection.ReplaceAll(Enumerable.Range(0, 10_000));

        Assert.Equal(1, resets);
        Assert.Equal(0, other);
        Assert.Equal(10_000, collection.Count);
        collection.ReplaceAll([1, 2]);
        Assert.Equal([1, 2], collection);
    }
}
