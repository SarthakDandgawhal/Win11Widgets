using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views;

public sealed partial class WidgetGalleryWindow : Window
{
    public WidgetGalleryViewModel ViewModel { get; }

    public WidgetGalleryWindow()
    {
        InitializeComponent();
        
        ViewModel = App.Current.Services.GetRequiredService<WidgetGalleryViewModel>();
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