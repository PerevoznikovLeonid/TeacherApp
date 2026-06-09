using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TeacherApp.Components;

public partial class InputComponent : UserControl
{
    public static readonly StyledProperty<object> LabelProperty =
        AvaloniaProperty.Register<InputComponent, object>(nameof(Label));
    public object Label 
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty,value);
        
    }
    
    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<InputComponent, string>(nameof(Value));
    public string Value 
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty,value);
    }
    
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<InputComponent, string?>(nameof(PlaceholderText));
    public string? PlaceholderText 
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty,value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<InputComponent, bool>(nameof(IsReadOnly));
    public bool IsReadOnly 
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public InputComponent()
    {
        InitializeComponent();
    }
}