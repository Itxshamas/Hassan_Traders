using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace HassanTraders.Desktop.Services;

/// <summary>
/// ObservableCollection that can swap all its items with one change notification.
/// Filtering a 10,000 row grid with Clear + Add would raise 10,000 events; this raises one.
/// The grid's sort order is kept because it is the same collection.
/// </summary>
public sealed class BulkObservableCollection<T> : ObservableCollection<T>
{
    public void ReplaceAll(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
