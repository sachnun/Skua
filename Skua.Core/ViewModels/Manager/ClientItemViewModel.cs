using CommunityToolkit.Mvvm.ComponentModel;

namespace Skua.Core.ViewModels;

public class ClientItemViewModel : ObservableObject
{
    public ClientItemViewModel()
    {
    }

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}