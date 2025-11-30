using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Skua.Core.Messaging;
using Skua.Core.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Skua.WPF.Views;

/// <summary>
/// Interaction logic for ScriptRepoView.xaml
/// </summary>
public partial class ScriptRepoView : UserControl
{
    private ICollectionView? _collectionView;
    private CancellationTokenSource? _searchCts;
    private string _lastSearchText = string.Empty;

    public ScriptRepoView()
    {
        InitializeComponent();
        
        Loaded += ScriptRepoView_Loaded;
        IsVisibleChanged += ScriptRepoView_IsVisibleChanged;
        Unloaded += (s, e) => StrongReferenceMessenger.Default.UnregisterAll(this);
    }

    private void ScriptRepoView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true && DataContext is ScriptRepoViewModel vm)
        {
            vm.Initialize();
        }
    }

    private void ScriptRepoView_Loaded(object sender, RoutedEventArgs e)
    {
        // Only set DataContext if not already set (allows parent to set it)
        if (DataContext is not ScriptRepoViewModel)
        {
            DataContext = Ioc.Default.GetRequiredService<ScriptRepoViewModel>();
        }
        
        if (DataContext is ScriptRepoViewModel vm)
        {
            _collectionView = CollectionViewSource.GetDefaultView(vm.Scripts);
            
            // Initialize/load scripts
            vm.Initialize();
        }
        
        // Register for close message (only once)
        if (!StrongReferenceMessenger.Default.IsRegistered<CloseScriptRepoMessage>(this))
        {
            StrongReferenceMessenger.Default.Register<CloseScriptRepoMessage>(this, (r, m) =>
            {
                Dispatcher.Invoke(() =>
                {
                    // Try to close window if in window mode, otherwise send toggle message
                    Window? window = Window.GetWindow(this);
                    if (window is not null && window.GetType().Name != "MainWindow")
                    {
                        window.Close();
                    }
                    else
                    {
                        // We're embedded in MainWindow, send message to hide
                        StrongReferenceMessenger.Default.Send(new ToggleScriptRepoMessage(false));
                    }
                });
            });
        }
    }

    private bool Search(object obj)
    {
        string searchScript = _lastSearchText;
        if (string.IsNullOrWhiteSpace(searchScript))
            return true;

        ScriptInfoViewModel? script = obj as ScriptInfoViewModel;
        if (script is null)
            return false;

        // Simple contains check - faster than KMP for short strings
        string scriptName = script.Info.Name;
        if (scriptName.Contains(searchScript, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check tags
        foreach (string tag in script.InfoTags)
        {
            if (tag.Contains(searchScript, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Cancel previous search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        CancellationToken token = _searchCts.Token;

        string searchText = SearchBox.Text.Trim().ToLowerInvariant();
        
        // Debounce - wait 200ms before searching
        try
        {
            await Task.Delay(200, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (token.IsCancellationRequested)
            return;

        _lastSearchText = searchText;

        // Apply filter on UI thread
        Dispatcher.Invoke(() =>
        {
            if (_collectionView is not null)
            {
                _collectionView.Filter = Search;
            }
        });
    }
}
