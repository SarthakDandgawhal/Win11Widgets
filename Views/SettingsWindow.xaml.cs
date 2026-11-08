using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views;

public sealed partial class SettingsWindow : Window
{
    public SettingsViewModel ViewModel { get; }

    public SettingsWindow()
    {
        InitializeComponent();
        
        ViewModel = App.Current.Services.GetRequiredService<SettingsViewModel>();
        DataContext = ViewModel;

        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

        SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}