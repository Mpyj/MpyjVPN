using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace MpyjVPN.UI.Controls;

public class PowerOrb : Button
{
    // ==================== PROPERTIES ====================
    
    public static readonly StyledProperty<bool> IsConnectedProperty =
        AvaloniaProperty.Register<PowerOrb, bool>(nameof(IsConnected));
    
    public static readonly StyledProperty<bool> IsConnectingProperty =
        AvaloniaProperty.Register<PowerOrb, bool>(nameof(IsConnecting));
    
    public bool IsConnected
    {
        get => GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }
    
    public bool IsConnecting
    {
        get => GetValue(IsConnectingProperty);
        set => SetValue(IsConnectingProperty, value);
    }
    
    // ==================== CONTROLS ====================
    
    private Ellipse? _outerRing;
    private Ellipse? _innerRing;
    private Ellipse? _core;
    private Ellipse? _glow;
    private TextBlock? _icon;
    
    // ==================== STATIC CTOR ====================
    
    static PowerOrb()
    {
        IsConnectedProperty.Changed.AddClassHandler<PowerOrb>((x, e) => x.UpdateState());
        IsConnectingProperty.Changed.AddClassHandler<PowerOrb>((x, e) => x.UpdateState());
    }
    
    // ==================== CONSTRUCTOR ====================
    
    public PowerOrb()
    {
        Width = 200;
        Height = 200;
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        Cursor = new Cursor(StandardCursorType.Hand);
        
        Content = CreateContent();
        
        // Hover effect
        PointerEntered += (s, e) => AnimateScale(1.05);
        PointerExited += (s, e) => AnimateScale(1.0);
    }
    
    // ==================== CONTENT ====================
    
    private Control CreateContent()
    {
        var grid = new Grid
        {
            Width = 200,
            Height = 200
        };
        
        // Glow (پشت همه - برای Connected)
        _glow = new Ellipse
        {
            Width = 200,
            Height = 200,
            Fill = new SolidColorBrush(Color.Parse("#10b981")),
            Opacity = 0,
            IsHitTestVisible = false
        };
        grid.Children.Add(_glow);
        
        // Outer Ring
        _outerRing = new Ellipse
        {
            Width = 180,
            Height = 180,
            Stroke = new SolidColorBrush(Color.Parse("#252535")),
            StrokeThickness = 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Fill = Brushes.Transparent
        };
        grid.Children.Add(_outerRing);
        
        // Inner Ring
        _innerRing = new Ellipse
        {
            Width = 130,
            Height = 130,
            Stroke = new SolidColorBrush(Color.Parse("#252535")),
            StrokeThickness = 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Fill = Brushes.Transparent
        };
        grid.Children.Add(_innerRing);
        
        // Core (دکمه وسط)
        _core = new Ellipse
        {
            Width = 90,
            Height = 90,
            Fill = new SolidColorBrush(Color.Parse("#1a1a28")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        grid.Children.Add(_core);
        
        // Icon
        _icon = new TextBlock
        {
            Text = "⚡",
            FontSize = 36,
            Foreground = new SolidColorBrush(Color.Parse("#606080")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        grid.Children.Add(_icon);
        
        return grid;
    }
    
    // ==================== STATE MANAGEMENT ====================
    
    private void UpdateState()
    {
        if (IsConnecting)
        {
            ApplyConnectingState();
        }
        else if (IsConnected)
        {
            ApplyConnectedState();
        }
        else
        {
            ApplyDisconnectedState();
        }
    }
    
    private void ApplyDisconnectedState()
    {
        if (_outerRing != null)
            _outerRing.Stroke = new SolidColorBrush(Color.Parse("#252535"));
        
        if (_innerRing != null)
            _innerRing.Stroke = new SolidColorBrush(Color.Parse("#252535"));
        
        if (_core != null)
            _core.Fill = new SolidColorBrush(Color.Parse("#1a1a28"));
        
        if (_glow != null)
            _glow.Opacity = 0;
        
        if (_icon != null)
        {
            _icon.Text = "⚡";
            _icon.FontSize = 36;
            _icon.Foreground = new SolidColorBrush(Color.Parse("#606080"));
        }
    }
    
    private void ApplyConnectingState()
    {
        if (_outerRing != null)
            _outerRing.Stroke = new SolidColorBrush(Color.Parse("#f59e0b"));
        
        if (_innerRing != null)
            _innerRing.Stroke = new SolidColorBrush(Color.Parse("#f59e0b"));
        
        if (_core != null)
            _core.Fill = new SolidColorBrush(Color.Parse("#2a1f0a"));
        
        if (_glow != null)
        {
            _glow.Fill = new SolidColorBrush(Color.Parse("#f59e0b"));
            _glow.Opacity = 0.2;
        }
        
        if (_icon != null)
        {
            _icon.Text = "⏳";
            _icon.FontSize = 36;
            _icon.Foreground = new SolidColorBrush(Color.Parse("#f59e0b"));
        }
        
        StartPulseAnimation(0.2, 0.5, TimeSpan.FromSeconds(1.2));
    }
    
    private void ApplyConnectedState()
    {
        if (_outerRing != null)
            _outerRing.Stroke = new SolidColorBrush(Color.Parse("#10b981"));
        
        if (_innerRing != null)
            _innerRing.Stroke = new SolidColorBrush(Color.Parse("#10b981"));
        
        if (_core != null)
            _core.Fill = new SolidColorBrush(Color.Parse("#10b981"));
        
        if (_glow != null)
        {
            _glow.Fill = new SolidColorBrush(Color.Parse("#10b981"));
            _glow.Opacity = 0.25;
        }
        
        if (_icon != null)
        {
            _icon.Text = "✓";
            _icon.FontSize = 42;
            _icon.Foreground = Brushes.White;
        }
        
        StartPulseAnimation(0.2, 0.4, TimeSpan.FromSeconds(1.5));
    }
    
    // ==================== ANIMATIONS ====================
    
    private void StartPulseAnimation(double from, double to, TimeSpan duration)
    {
        if (_glow == null) return;
        
        var animation = new Animation
        {
            Duration = duration,
            IterationCount = IterationCount.Infinite,
            Easing = new SineEaseInOut()
        };
        
        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0),
            Setters = { new Setter(OpacityProperty, from) }
        });
        
        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0.5),
            Setters = { new Setter(OpacityProperty, to) }
        });
        
        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1),
            Setters = { new Setter(OpacityProperty, from) }
        });
        
        _glow.Opacity = from;
        _ = animation.RunAsync(_glow);
    }
    
    private void AnimateScale(double scale)
    {
        if (Content is Grid grid)
        {
            var transform = grid.RenderTransform as ScaleTransform 
                ?? new ScaleTransform(1, 1);
            transform.ScaleX = scale;
            transform.ScaleY = scale;
            grid.RenderTransform = transform;
            grid.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        }
    }
}