using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Windowing;
using Win11Widgets.ViewModels;
using Win11Widgets.Models;
using Windows.Foundation;

namespace Win11Widgets.Views;

public sealed partial class OverlayWindow : Window
{
    public OverlayViewModel ViewModel { get; }

    public OverlayWindow()
    {
        InitializeComponent();
        
        ViewModel = App.Current.Services.GetRequiredService<OverlayViewModel>();
        DataContext = ViewModel;

        // Configure window for overlay behavior
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        
        // Make window click-through when no widgets
        // But allow interaction with widgets
        ExtendsContentIntoTitleBar = true;
        SystemBackdrop = null;

        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Position window to cover full screen
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var workArea = displayArea.WorkArea;
        
        AppWindow.MoveAndResize(new RectInt32(workArea.X, workArea.Y, workArea.Width, workArea.Height));
        
        // Set as overlay window (always on top, click-through when empty)
        AppWindow.Presenter.Kind = AppWindowPresenterKind.Overlay;
        
        UpdateEmptyState();
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        // App is closing
    }

    private void UpdateEmptyState()
    {
        var hasWidgets = ViewModel.Widgets.Count > 0;
        EmptyStatePanel.Visibility = hasWidgets ? Visibility.Collapsed : Visibility.Visible;
        QuickActionsBar.Visibility = hasWidgets ? Visibility.Visible : Visibility.Collapsed;
        FabButton.Visibility = hasWidgets ? Visibility.Visible : Visibility.Collapsed;
    }

    private void FabButton_Click(object sender, RoutedEventArgs e)
    {
        ShowWidgetMenu();
    }

    private void ShowWidgetMenu()
    {
        var menu = new MenuFlyout();
        
        var categories = ViewModel.WidgetManager.WidgetDefinitions
            .GroupBy(w => w.Category)
            .OrderBy(g => g.Key);

        foreach (var category in categories)
        {
            var subMenu = new MenuFlyoutSubItem { Text = category.Key };
            
            foreach (var widget in category.OrderBy(w => w.Name))
            {
                var item = new MenuFlyoutItem
                {
                    Text = widget.Name,
                    Icon = new FontIconSource { Glyph = widget.IconGlyph }
                };
                item.Click += (s, e) => ViewModel.CreateWidget(widget.Type);
                subMenu.Items.Add(item);
            }
            
            menu.Items.Add(subMenu);
        }

        menu.ShowAt(FabButton);
    }

    private void NewStickyNote_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CreateWidget(WidgetType.StickyNote);
        UpdateEmptyState();
    }

    private void NewClock_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CreateWidget(WidgetType.Clock);
        UpdateEmptyState();
    }

    private void NewTodo_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CreateWidget(WidgetType.TodoList);
        UpdateEmptyState();
    }

    private void OpenGallery_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowGallery();
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowSettings();
    }

    public new void Hide()
    {
        base.Hide();
        ViewModel.IsVisible = false;
    }

    public new void Show()
    {
        base.Show();
        ViewModel.IsVisible = true;
        UpdateEmptyState();
    }
}