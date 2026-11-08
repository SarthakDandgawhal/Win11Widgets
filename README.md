# Win11 Widgets Manager

A native Windows 11 desktop widgets manager built with WinUI 3 and Windows App SDK. Features drag-and-drop widgets that live on your desktop, similar to the classic Windows gadgets but modern and performant.

## Features

- **Drag & Drop**: Move widgets anywhere on your desktop
- **Resize**: Drag the corner handle to resize widgets
- **Pin**: Keep widgets always on top
- **System Tray**: Access widgets from the system tray
- **Auto-start**: Launch automatically with Windows
- **Persistence**: Widget positions and state saved automatically
- **Multiple Widget Types**:
  - Sticky Notes (with color support)
  - Clock (digital, 12/24 hour, seconds)
  - Weather (current + 3-day forecast)
  - System Monitor (CPU, Memory, Disk, Network graphs)
  - Todo List (with priorities, due dates, drag reorder)
  - Calendar (monthly view)
  - Calculator (standard operations + history)
  - Color Picker (RGB, HSL, Hex, CMYK + palette)

## Requirements

- Windows 11 (build 22000+) or Windows 10 (build 19041+)
- .NET 8 SDK
- Visual Studio 2022 17.8+ with Windows App SDK workload

## Building

```powershell
# Restore packages
dotnet restore

# Build
dotnet build -c Release

# Run
dotnet run --project Win11Widgets/Win11Widgets.csproj

# Publish (self-contained)
dotnet publish -c Release -r win10-x64 --self-contained true
```

## Installation

1. Build the project in Release mode
2. Run the published `.exe` from `bin/Release/net8.0-windows10.0.19041.0/win10-x64/publish/`
3. Or create an MSIX package for distribution

## Usage

- **Win+Shift+W**: Toggle widgets overlay
- **Win+Shift+N**: Create new widget
- **Win+Shift+S**: Open settings
- **Right-click tray icon**: Context menu with quick actions
- **Drag widgets**: Click and drag widget header to move
- **Resize**: Drag bottom-right corner handle
- **Pin**: Click pin icon to keep on top

## Architecture

```
Win11Widgets/
├── Models/           # Widget state models & definitions
├── Services/         # Core services (WidgetManager, Persistence, Settings, SystemTray)
├── ViewModels/       # MVVM view models for each widget type
├── Views/            # XAML views
│   ├── Widgets/      # Individual widget views
│   ├── OverlayWindow.xaml      # Main overlay window
│   ├── SettingsWindow.xaml     # Settings dialog
│   └── WidgetGalleryWindow.xaml # Widget gallery
├── Messages/         # MVVM messaging
├── Converters/       # XAML value converters
└── Assets/           # Icons and images
```

## Widget Development

To add a new widget:

1. Add `WidgetType` enum value in `Models/WidgetModels.cs`
2. Create widget state class inheriting from `WidgetState`
3. Create `WidgetViewModelBase` derived view model
4. Create XAML view in `Views/Widgets/`
5. Register in `WidgetManagerService.RegisterBuiltInWidgets()`
6. Add DataTemplate in `OverlayWindow.xaml` resources
7. Add template selector case in `WidgetTemplateSelector.cs`

## Settings

All settings stored in `%LOCALAPPDATA%\Win11Widgets\settings.json`
Widget states stored in `%LOCALAPPDATA%\Win11Widgets\Widgets\`

## License

MIT License