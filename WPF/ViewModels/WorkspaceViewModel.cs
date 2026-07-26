using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class WorkspaceViewModel : BaseViewModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserActivityService _userActivityService;

    private ActivityDashboardDto _dashboard = new();
    private string _message = string.Empty;
    private bool _isBusy;

    public WorkspaceViewModel(
        ICurrentUserService currentUserService,
        IUserActivityService userActivityService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _userActivityService = userActivityService ?? throw new ArgumentNullException(nameof(userActivityService));
        _currentUserService.SessionChanged += OnSessionChanged;

        RefreshCommand = new AsyncRelayCommand(LoadDashboardAsync, () => !IsBusy && IsAdmin);
    }

    public override string Title => "Workspace";

    public override string Description => IsAdmin
        ? "System activity dashboard and recent audit events"
        : "Role-based hotel workspace";

    public ActivityDashboardDto Dashboard
    {
        get => _dashboard;
        private set => SetProperty(ref _dashboard, value);
    }

    public bool IsAdmin => _currentUserService.HasRole(RoleName.Admin);

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public override void OnNavigatedTo()
    {
        if (IsAdmin)
        {
            _ = LoadDashboardAsync();
        }
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        OnPropertiesChanged(nameof(IsAdmin), nameof(Description));
        RefreshCommand.RaiseCanExecuteChanged();

        if (IsAdmin)
        {
            _ = LoadDashboardAsync();
        }
        else
        {
            Dashboard = new ActivityDashboardDto();
        }
    }

    private async Task LoadDashboardAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        IsBusy = true;
        Message = string.Empty;

        try
        {
            var result = await _userActivityService.GetActivityDashboardAsync(_currentUserService.User);
            if (result.IsSuccess)
            {
                Dashboard = result.Data ?? new ActivityDashboardDto();
            }
            else
            {
                Message = result.Errors.FirstOrDefault() ?? result.Message;
            }
        }
        catch (Exception ex)
        {
            Message = $"Unable to load workspace dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
