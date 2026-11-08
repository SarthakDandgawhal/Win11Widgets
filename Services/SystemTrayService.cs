using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Win11Widgets.Services;

[SupportedOSPlatform("windows10.0.19041.0")]
public class SystemTrayService : IDisposable
{
    private readonly ILogger<SystemTrayService> _logger;
    private readonly SettingsService _settings;
    private App? _app;
    private NOTIFYICONDATA _notifyIconData;
    private bool _isInitialized = false;
    private WNDCLASSEX _windowClass;
    private HWND _messageWindow;
    private readonly Dictionary<uint, Action> _commandHandlers = new();
    private const uint WM_TRAYICON = 0x0400 + 1;
    private const uint WM_TASKBARCREATED = 0x0400 + 2;
    private const int ID_TRAY_ICON = 1;
    private const int CMD_SHOW = 1001;
    private const int CMD_HIDE = 1002;
    private const int CMD_SETTINGS = 1003;
    private const int CMD_GALLERY = 1004;
    private const int CMD_EXIT = 1005;
    private const int CMD_NEW_STICKY = 1006;
    private const int CMD_NEW_CLOCK = 1007;
    private const int CMD_NEW_TODO = 1008;

    public SystemTrayService(ILogger<SystemTrayService> logger, SettingsService settings)
    {
        _logger = logger;
        _settings = settings;
        _settings.SettingsChanged += OnSettingsChanged;
    }

    public void Initialize(App app)
    {
        _app = app;
        CreateMessageWindow();
        CreateTrayIcon();
        _isInitialized = true;
    }

    private void CreateMessageWindow()
    {
        var className = "Win11WidgetsTrayWindow";
        
        _windowClass = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = WndProc,
            hInstance = GetModuleHandle(null),
            lpszClassName = className,
            hIcon = LoadIcon(GetModuleHandle(null), IDI_APPLICATION),
            hCursor = LoadCursor(0, IDC_ARROW),
            hbrBackground = (HBRUSH)(COLOR_WINDOW + 1)
        };

        var atom = RegisterClassEx(ref _windowClass);
        if (atom == 0)
        {
            var error = Marshal.GetLastWin32Error();
            _logger.LogError("Failed to register window class: {Error}", error);
            return;
        }

        _messageWindow = CreateWindowEx(
            0,
            className,
            "Win11WidgetsTray",
            0,
            0, 0, 0, 0,
            HWND.NULL,
            HMENU.NULL,
            GetModuleHandle(null),
            null);

        if (_messageWindow == HWND.NULL)
        {
            var error = Marshal.GetLastWin32Error();
            _logger.LogError("Failed to create message window: {Error}", error);
        }
    }

    private void CreateTrayIcon()
    {
        _notifyIconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _messageWindow,
            uID = ID_TRAY_ICON,
            uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP | NIF_SHOWTIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = LoadIcon(GetModuleHandle(null), IDI_APPLICATION),
            szTip = "Win11 Widgets Manager",
            dwState = 0,
            dwStateMask = 0
        };

        Shell_NotifyIcon(NIM_ADD, ref _notifyIconData);
        Shell_NotifyIcon(NIM_SETVERSION, ref _notifyIconData);
    }

    private void OnSettingsChanged(WidgetSettings settings)
    {
        if (!_isInitialized) return;

        if (settings.ShowInSystemTray && _notifyIconData.hIcon != 0)
        {
            // Already visible
        }
        else if (!settings.ShowInSystemTray)
        {
            Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
        }
    }

    private void ShowContextMenu()
    {
        if (_messageWindow == HWND.NULL) return;

        var menu = CreatePopupMenu();
        if (menu == HMENU.NULL) return;

        // Get cursor position
        GetCursorPos(out POINT pt);

        // Set foreground window to ensure menu closes properly
        SetForegroundWindow(_messageWindow);

        var cmd = TrackPopupMenu(
            menu,
            TPM_RETURNCMD | TPM_RIGHTBUTTON | TPM_LEFTALIGN | TPM_BOTTOMALIGN,
            pt.X, pt.Y,
            0,
            _messageWindow,
            null);

        if (cmd != 0 && _commandHandlers.TryGetValue(cmd, out var handler))
        {
            handler();
        }

        DestroyMenu(menu);
        PostMessage(_messageWindow, WM_NULL, 0, 0);
    }

    private HMENU CreatePopupMenu()
    {
        var menu = CreatePopupMenu();
        if (menu == HMENU.NULL) return HMENU.NULL;

        _commandHandlers.Clear();

        // Show/Hide Overlay
        AppendMenu(menu, MF_STRING, CMD_SHOW, "Show Widgets Overlay");
        _commandHandlers[CMD_SHOW] = () => _app?.ToggleOverlay();

        AppendMenu(menu, MF_STRING, CMD_HIDE, "Hide Widgets Overlay");
        _commandHandlers[CMD_HIDE] = () => _app?.ToggleOverlay();

        AppendMenu(menu, MF_SEPARATOR, 0, "");

        // New Widgets submenu
        var widgetMenu = CreatePopupMenu();
        if (widgetMenu != HMENU.NULL)
        {
            AppendMenu(widgetMenu, MF_STRING, CMD_NEW_STICKY, "Sticky Note");
            _commandHandlers[CMD_NEW_STICKY] = () => _app?.WidgetManager.CreateWidget(WidgetType.StickyNote);

            AppendMenu(widgetMenu, MF_STRING, CMD_NEW_CLOCK, "Clock");
            _commandHandlers[CMD_NEW_CLOCK] = () => _app?.WidgetManager.CreateWidget(WidgetType.Clock);

            AppendMenu(widgetMenu, MF_STRING, CMD_NEW_TODO, "Todo List");
            _commandHandlers[CMD_NEW_TODO] = () => _app?.WidgetManager.CreateWidget(WidgetType.TodoList);

            AppendMenu(menu, MF_POPUP, (uint)widgetMenu, "New Widget");
        }

        AppendMenu(menu, MF_SEPARATOR, 0, "");

        AppendMenu(menu, MF_STRING, CMD_GALLERY, "Widget Gallery...");
        _commandHandlers[CMD_GALLERY] = () => _app?.ShowWidgetGallery();

        AppendMenu(menu, MF_STRING, CMD_SETTINGS, "Settings...");
        _commandHandlers[CMD_SETTINGS] = () => _app?.ShowSettings();

        AppendMenu(menu, MF_SEPARATOR, 0, "");

        AppendMenu(menu, MF_STRING, CMD_EXIT, "Exit");
        _commandHandlers[CMD_EXIT] = () => _app?.Exit();

        return menu;
    }

    private LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case WM_TRAYICON:
                switch (lParam)
                {
                    case WM_RBUTTONUP:
                    case WM_CONTEXTMENU:
                        ShowContextMenu();
                        break;
                    case WM_LBUTTONDBLCLK:
                        _app?.ToggleOverlay();
                        break;
                }
                break;

            case WM_TASKBARCREATED:
                CreateTrayIcon();
                break;

            case WM_DESTROY:
                Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
                PostQuitMessage(0);
                break;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_isInitialized)
        {
            Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
            if (_messageWindow != HWND.NULL)
            {
                DestroyWindow(_messageWindow);
            }
            UnregisterClass(_windowClass.lpszClassName, _windowClass.hInstance);
            _isInitialized = false;
        }
    }

    // P/Invoke signatures
    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(Utf16StringMarshaller))]
    private static partial ATOM RegisterClassEx([In] ref WNDCLASSEX lpwcx);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(Utf16StringMarshaller))]
    private static partial HWND CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        HWND hWndParent,
        HMENU hMenu,
        HINSTANCE hInstance,
        void* lpParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL DestroyWindow(HWND hWnd);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(Utf16StringMarshaller))]
    private static partial BOOL UnregisterClass(string lpClassName, HINSTANCE hInstance);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial HINSTANCE GetModuleHandle(string? lpModuleName);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial HICON LoadIcon(HINSTANCE hInstance, IntPtr lpIconName);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial HCURSOR LoadCursor(HINSTANCE hInstance, IntPtr lpCursorName);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL GetCursorPos(out POINT lpPoint);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL SetForegroundWindow(HWND hWnd);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial HMENU CreatePopupMenu();

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL AppendMenu(HMENU hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int TrackPopupMenu(HMENU hMenu, uint uFlags, int x, int y, int nReserved, HWND hWnd, RECT? prcRect);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL DestroyMenu(HMENU hMenu);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL PostMessage(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial LRESULT DefWindowProc(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial void PostQuitMessage(int nExitCode);

    [LibraryImport("shell32.dll", SetLastError = true)]
    private static partial BOOL Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    // Constants
    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_MODIFY = 0x00000001;
    private const uint NIM_DELETE = 0x00000002;
    private const uint NIM_SETVERSION = 0x00000004;
    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint NIF_SHOWTIP = 0x00000080;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint WM_LBUTTONDBLCLK = 0x0203;
    private const uint WM_CONTEXTMENU = 0x007B;
    private const uint WM_NULL = 0x0000;
    private const uint WM_DESTROY = 0x0002;
    private const int IDI_APPLICATION = 32512;
    private const int IDC_ARROW = 32512;
    private const int COLOR_WINDOW = 5;
    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint MF_POPUP = 0x00000010;
    private const uint TPM_RETURNCMD = 0x0100;
    private const uint TPM_RIGHTBUTTON = 0x0002;
    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint TPM_BOTTOMALIGN = 0x0020;

    // Structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        public delegate* unmanaged[Cdecl]<HWND, uint, WPARAM, LPARAM, LRESULT> lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public HINSTANCE hInstance;
        public HICON hIcon;
        public HCURSOR hCursor;
        public HBRUSH hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public HICON hIconSm;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public uint cbSize;
        public HWND hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public HICON hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeout;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public HICON hBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private delegate LRESULT WndProcDelegate(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam);
}