namespace WPF.ViewModels;

public sealed class ReportModuleViewModel : BaseViewModel
{
    private bool _isSelected;

    public ReportModuleViewModel(
        string key,
        string title,
        string description,
        string iconKind,
        BaseViewModel viewModel)
    {
        Key = key;
        Title = title;
        Description = description;
        IconKind = iconKind;
        ViewModel = viewModel;
    }

    public string Key { get; }

    public override string Title { get; }

    public override string Description { get; }

    public string IconKind { get; }

    public BaseViewModel ViewModel { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
