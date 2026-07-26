using System.Collections.ObjectModel;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class AdminSetupModuleViewModel : BaseViewModel
{
    private bool _isSelected;

    public AdminSetupModuleViewModel(
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

public sealed class AdminSetupViewModel : BaseViewModel
{
    private readonly RoomTypeManagementViewModel _roomTypeManagementViewModel;
    private readonly RoomManagementViewModel _roomManagementViewModel;
    private readonly ServiceManagementViewModel _serviceManagementViewModel;
    private readonly HashSet<string> _initializedModuleKeys = new(StringComparer.OrdinalIgnoreCase);

    private BaseViewModel _currentViewModel;
    private AdminSetupModuleViewModel? _selectedModule;
    private string _setupMessage = string.Empty;
    private bool _isBusy;

    public AdminSetupViewModel(
        RoomTypeManagementViewModel roomTypeManagementViewModel,
        RoomManagementViewModel roomManagementViewModel,
        ServiceManagementViewModel serviceManagementViewModel)
    {
        _roomTypeManagementViewModel = roomTypeManagementViewModel
            ?? throw new ArgumentNullException(nameof(roomTypeManagementViewModel));
        _roomManagementViewModel = roomManagementViewModel
            ?? throw new ArgumentNullException(nameof(roomManagementViewModel));
        _serviceManagementViewModel = serviceManagementViewModel
            ?? throw new ArgumentNullException(nameof(serviceManagementViewModel));

        _currentViewModel = _roomTypeManagementViewModel;
        Modules = new ObservableCollection<AdminSetupModuleViewModel>(CreateModules());
        SelectModuleCommand = new RelayCommand<AdminSetupModuleViewModel>(SelectModule);

        if (Modules.FirstOrDefault() is { } firstModule)
        {
            SetSelectedModule(firstModule);
        }
    }

    public override string Title => "Admin Setup";

    public override string Description => "Room inventory, room types and service catalog setup";

    public ObservableCollection<AdminSetupModuleViewModel> Modules { get; }

    public RelayCommand<AdminSetupModuleViewModel> SelectModuleCommand { get; }

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public AdminSetupModuleViewModel? SelectedModule
    {
        get => _selectedModule;
        private set
        {
            if (SetProperty(ref _selectedModule, value))
            {
                OnPropertiesChanged(nameof(CurrentModuleTitle), nameof(CurrentModuleDescription));
            }
        }
    }

    public string CurrentModuleTitle => SelectedModule?.Title ?? Title;

    public string CurrentModuleDescription => SelectedModule?.Description ?? Description;

    public string SetupMessage
    {
        get => _setupMessage;
        private set => SetProperty(ref _setupMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public override async Task InitializeAsync()
    {
        await InitializeSelectedModuleAsync();
    }

    public override void OnNavigatedTo()
    {
        _ = InitializeSelectedModuleAsync();
    }

    private IEnumerable<AdminSetupModuleViewModel> CreateModules()
    {
        return
        [
            new AdminSetupModuleViewModel(
                "room-types",
                "Room Types",
                "Manage room categories, price and capacity.",
                "FileDocumentPlus",
                _roomTypeManagementViewModel),
            new AdminSetupModuleViewModel(
                "rooms",
                "Rooms",
                "Manage room inventory and operating status.",
                "OfficeBuilding",
                _roomManagementViewModel),
            new AdminSetupModuleViewModel(
                "services",
                "Services",
                "Maintain the hotel service catalog.",
                "InformationOutline",
                _serviceManagementViewModel)
        ];
    }

    private async void SelectModule(AdminSetupModuleViewModel? module)
    {
        if (module is null)
        {
            return;
        }

        try
        {
            SetSelectedModule(module);
            await InitializeSelectedModuleAsync();
        }
        catch (Exception ex)
        {
            SetupMessage = $"Unable to open {module.Title}: {ex.Message}";
        }
    }

    private void SetSelectedModule(AdminSetupModuleViewModel module)
    {
        foreach (var item in Modules)
        {
            item.IsSelected = ReferenceEquals(item, module);
        }

        SelectedModule = module;
        CurrentViewModel = module.ViewModel;
        SetupMessage = string.Empty;

        module.ViewModel.OnNavigatedTo();
    }

    private async Task InitializeSelectedModuleAsync()
    {
        if (SelectedModule is null || _initializedModuleKeys.Contains(SelectedModule.Key))
        {
            return;
        }

        IsBusy = true;
        SetupMessage = string.Empty;

        try
        {
            await SelectedModule.ViewModel.InitializeAsync();
            _initializedModuleKeys.Add(SelectedModule.Key);
        }
        catch (Exception ex)
        {
            SetupMessage = $"Unable to load {SelectedModule.Title}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
