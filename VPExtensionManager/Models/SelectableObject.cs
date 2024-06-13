using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VPExtensionManager.Models;

public class SelectableObject<T> : INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }

    private T _data;
    public T Data
    {
        get => _data;
        set => Set(ref _data, value);
    }

    public SelectableObject(T data)
    {
        Data = data;
    }

    public SelectableObject(T data, bool isSelected)
    {
        IsSelected = isSelected;
        Data = data;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private void Set<TT>(ref TT storage, TT value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }
}
