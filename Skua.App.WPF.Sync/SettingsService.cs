using Skua.Core.Interfaces;
using Skua.Core.Models;

namespace Skua.App.WPF.Sync;

public class SettingsService : ISettingsService
{
    public T? Get<T>(string key)
    {
        return default;
    }

    public T Get<T>(string key, T defaultValue)
    {
        var suco = default(T);

        return suco is null ? defaultValue : suco;
    }

    public void Set<T>(string key, T value)
    {
    }

    public void Initialize(AppRole role)
    {
    }

    public SharedSettings GetShared()
    {
        return new SharedSettings();
    }

    public ClientSettings GetClient()
    {
        return new ClientSettings();
    }

    public ManagerSettings GetManager()
    {
        return new ManagerSettings();
    }

    public void SetApplicationVersion(string version)
    {
    }
}