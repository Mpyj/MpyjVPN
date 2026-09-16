using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace MpyjVPN.UI.Controls;

public class ParticleCanvas : Control
{
    private class Particle
    {
        public double X, Y, VX, VY, Size;
        public Color Color;
    }
    
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    private readonly DispatcherTimer _timer;
    
    public ParticleCanvas()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(66) // 15 FPS
        };
        _timer.Tick += (s, e) => UpdateParticles();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        // ساخت ۲۰ ذره
        for (int i = 0; i < 20; i++)
        {
            _particles.Add(new Particle
            {
                X = _random.NextDouble() * Bounds.Width,
                Y = _random.NextDouble() * Bounds.Height,
                VX = (_random.NextDouble() - 0.5) * 0.3,
                VY = (_random.NextDouble() - 0.5) * 0.3,
                Size = _random.NextDouble() * 1.5 + 0.5,
                Color = _random.Next(2) == 0 
                    ? Color.FromArgb(80, 0, 229, 255) 
                    : Color.FromArgb(80, 139, 92, 246)
            });
        }
        
        _timer.Start();
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        base.OnDetachedFromVisualTree(e);
    }
    
    private void UpdateParticles()
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        
        foreach (var p in _particles)
        {
            p.X += p.VX;
            p.Y += p.VY;
            
            if (p.X < 0) p.X = width;
            else if (p.X > width) p.X = 0;
            if (p.Y < 0) p.Y = height;
            else if (p.Y > height) p.Y = 0;
        }
        
        InvalidateVisual();
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        foreach (var p in _particles)
        {
            context.DrawEllipse(
                new SolidColorBrush(p.Color),
                null,
                new Point(p.X, p.Y),
                p.Size,
                p.Size
            );
        }
    }
}