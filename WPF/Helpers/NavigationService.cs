using WPF.ViewModels;

namespace WPF.Helpers;

public sealed class NavigationService
{
    private readonly Dictionary<string, Func<BaseViewModel>> _factories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _history = new();

    public event EventHandler? NavigationChanged;

    public BaseViewModel? CurrentViewModel { get; private set; }

    public string? CurrentKey { get; private set; }

    public bool CanGoBack => _history.Count > 0;

    public void Register(string key, Func<BaseViewModel> viewModelFactory)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Navigation key cannot be empty.", nameof(key));
        }

        _factories[key] = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
    }

    public bool Navigate(string key, bool addToHistory = true)
    {
        if (!_factories.TryGetValue(key, out var viewModelFactory))
        {
            throw new InvalidOperationException($"No view model was registered for navigation key '{key}'.");
        }

        if (string.Equals(CurrentKey, key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (addToHistory && CurrentKey is not null)
        {
            _history.Push(CurrentKey);
        }

        CurrentViewModel?.OnNavigatedFrom();

        CurrentViewModel = viewModelFactory();
        CurrentKey = key;

        CurrentViewModel.OnNavigatedTo();
        NavigationChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public bool GoBack()
    {
        if (_history.Count == 0)
        {
            return false;
        }

        var previousKey = _history.Pop();
        return Navigate(previousKey, addToHistory: false);
    }
}
