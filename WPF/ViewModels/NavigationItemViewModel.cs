using BusinessObjects.Enums;

namespace WPF.ViewModels;

public sealed class NavigationItemViewModel : BaseViewModel
{
    private bool _isVisible = true;
    private bool _isSelected;

    public NavigationItemViewModel(
        string key,
        string title,
        string caption,
        string iconKind,
        IEnumerable<RoleName>? allowedRoles = null)
    {
        Key = key;
        TitleText = title;
        Caption = caption;
        IconKind = iconKind;
        AllowedRoles = (allowedRoles ?? Array.Empty<RoleName>()).Distinct().ToArray();
    }

    public string Key { get; }

    public string TitleText { get; }

    public string Caption { get; }

    public string IconKind { get; }

    public IReadOnlyCollection<RoleName> AllowedRoles { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsAllowedFor(RoleName? roleName)
    {
        return AllowedRoles.Count == 0 || (roleName.HasValue && AllowedRoles.Contains(roleName.Value));
    }
}
