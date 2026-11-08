using CommunityToolkit.Mvvm.Messaging;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Messages;

public class WidgetCreatedMessage : ValueChangedMessage<WidgetViewModelBase>
{
    public WidgetCreatedMessage(WidgetViewModelBase widget) : base(widget) { }
}

public class WidgetRemovedMessage : ValueChangedMessage<string>
{
    public WidgetRemovedMessage(string widgetId) : base(widgetId) { }
}

public class WidgetUpdatedMessage : ValueChangedMessage<WidgetViewModelBase>
{
    public WidgetUpdatedMessage(WidgetViewModelBase widget) : base(widget) { }
}

public class WidgetPositionChangedMessage : ValueChangedMessage<(string WidgetId, double X, double Y)>
{
    public WidgetPositionChangedMessage((string WidgetId, double X, double Y) value) : base(value) { }
}

public class WidgetSizeChangedMessage : ValueChangedMessage<(string WidgetId, double Width, double Height)>
{
    public WidgetSizeChangedMessage((string WidgetId, double Width, double Height) value) : base(value) { }
}

public class WidgetZIndexChangedMessage : ValueChangedMessage<(string WidgetId, int ZIndex)>
{
    public WidgetZIndexChangedMessage((string WidgetId, int ZIndex) value) : base(value) { }
}

public class OverlayVisibilityChangedMessage : ValueChangedMessage<bool>
{
    public OverlayVisibilityChangedMessage(bool isVisible) : base(isVisible) { }
}

public class SettingsOpenedMessage { }
public class GalleryOpenedMessage { }
public class NewWidgetRequestedMessage : ValueChangedMessage<WidgetType>
{
    public NewWidgetRequestedMessage(WidgetType type) : base(type) { }
}