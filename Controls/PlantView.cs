using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Seed.Controls;

public sealed class PlantView : FrameworkElement
{
    private readonly DispatcherTimer _timer;
    private double _phase;
    private double _wither;

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(int), typeof(PlantView),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

    public int Level
    {
        get => (int)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public PlantView()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += (_, _) =>
        {
            _phase += .035;
            if (_wither > 0) _wither = Math.Min(1, _wither + .018);
            InvalidateVisual();
        };
        _timer.Start();
    }

    public void PlayWither()
    {
        _wither = .01;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var w = ActualWidth;
        var h = ActualHeight;
        if (w < 10 || h < 10) return;

        var sway = Math.Sin(_phase) * (4 + Level * .25);
        var cx = w / 2;
        var baseY = h * .84;
        var growth = Math.Min(1, .38 + Level * .075);
        var topY = baseY - h * growth;
        var wiltDrop = _wither * h * .16;
        var green = ColorExtensions.Lerp(Color.FromRgb(48, 126, 82), Color.FromRgb(127, 105, 72), (float)_wither);
        var dark = ColorExtensions.Lerp(Color.FromRgb(34, 91, 62), Color.FromRgb(92, 75, 55), (float)_wither);

        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(30, 40, 60, 45)), null,
            new Point(cx, baseY + 18), w * .22, h * .035);

        var pot = new StreamGeometry();
        using (var g = pot.Open())
        {
            g.BeginFigure(new Point(cx - w * .13, baseY - 4), true, true);
            g.LineTo(new Point(cx + w * .13, baseY - 4), true, false);
            g.LineTo(new Point(cx + w * .09, baseY + h * .12), true, false);
            g.LineTo(new Point(cx - w * .09, baseY + h * .12), true, false);
        }
        dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(221, 121, 76)), null, pot);
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(238, 145, 93)), null,
            new Rect(cx - w * .15, baseY - 11, w * .3, 22), 7, 7);

        var stemPen = new Pen(new SolidColorBrush(dark), Math.Max(5, w * .022))
        { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var stem = new BezierSegment(
            new Point(cx - sway * .2, baseY - (baseY - topY) * .35),
            new Point(cx + sway * .7, topY + (baseY - topY) * .25),
            new Point(cx + sway, topY + wiltDrop), true);
        var figure = new PathFigure(new Point(cx, baseY), [stem], false);
        dc.DrawGeometry(null, stemPen, new PathGeometry([figure]));

        var leafCount = Math.Min(8, Math.Max(0, Level - 1));
        for (var i = 0; i < leafCount; i++)
        {
            var t = .2 + i * .095;
            var y = baseY - (baseY - topY) * t + _wither * 10;
            var side = i % 2 == 0 ? -1 : 1;
            var x = cx + sway * t + side * w * (.08 + i * .004);
            var angle = side * (25 + Math.Sin(_phase + i) * 4) + _wither * side * 55;
            DrawLeaf(dc, new Point(x, y), w * (.075 + Level * .003), h * .055, angle, green);
            dc.DrawLine(new Pen(new SolidColorBrush(dark), 2),
                new Point(cx + sway * t, y + 5), new Point(x, y));
        }

        if (Level >= 3) DrawLeaf(dc, new Point(cx + sway, topY + wiltDrop), w * .10, h * .07,
            Math.Sin(_phase) * 5 + _wither * 80, green);

        if (Level >= 7 && _wither < .7)
        {
            var bloom = Level >= 8 ? 18 : 12;
            for (var i = 0; i < 6; i++)
            {
                var a = i * Math.PI / 3 + _phase * .08;
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(244, 157, 156)), null,
                    new Point(cx + sway + Math.Cos(a) * bloom, topY + wiltDrop + Math.Sin(a) * bloom),
                    bloom * .7, bloom);
            }
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(248, 201, 94)), null,
                new Point(cx + sway, topY + wiltDrop), 9, 9);
        }
    }

    private static void DrawLeaf(DrawingContext dc, Point center, double rx, double ry, double angle, Color color)
    {
        dc.PushTransform(new RotateTransform(angle, center.X, center.Y));
        var geo = new StreamGeometry();
        using (var g = geo.Open())
        {
            g.BeginFigure(new Point(center.X - rx, center.Y), true, true);
            g.BezierTo(new Point(center.X - rx * .3, center.Y - ry), new Point(center.X + rx * .7, center.Y - ry), new Point(center.X + rx, center.Y), true, false);
            g.BezierTo(new Point(center.X + rx * .4, center.Y + ry), new Point(center.X - rx * .5, center.Y + ry * .7), new Point(center.X - rx, center.Y), true, false);
        }
        dc.DrawGeometry(new SolidColorBrush(color), null, geo);
        dc.Pop();
    }
}

internal static class ColorExtensions
{
    public static Color Lerp(Color a, Color b, float t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));
}
