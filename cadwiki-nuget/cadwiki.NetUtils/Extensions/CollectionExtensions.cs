using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

public static class CollectionExtensions
{
    public static void ClearAndAddRange<T>(this ObservableCollection<T> observableCollection, List<T> items)
    {
        // Clear the existing items in the ObservableCollection
        observableCollection.Clear();

        // Add each item from the List to the ObservableCollection
        foreach (var item in items)
        {
            observableCollection.Add(item);
        }
    }
}

public class AlphaAutoSortObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
{
    public AlphaAutoSortObservableCollection() : base() { }

    public AlphaAutoSortObservableCollection(IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            Add(item);
        }
        Sort();
    }

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        Sort();
    }

    private void Sort()
    {
        List<T> sortedList = new List<T>(this);
        sortedList.Sort();
        for (int i = 0; i < sortedList.Count; i++)
        {
            Move(IndexOf(sortedList[i]), i);
        }
    }
}