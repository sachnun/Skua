using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Skua.Core.Messaging;

namespace Skua.Core.ViewModels;

public partial class AccountItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    private bool _useCheck;

    public bool UseCheck
    {
        get => _useCheck;
        set
        {
            if (SetProperty(ref _useCheck, value))
                WeakReferenceMessenger.Default.Send<AccountSelectedMessage>(new(_useCheck));
        }
    }

    [RelayCommand]
    private void Remove()
    {
        WeakReferenceMessenger.Default.Send<RemoveAccountMessage>(new(this));
    }
}