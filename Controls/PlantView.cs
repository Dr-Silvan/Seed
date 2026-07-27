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
        set => SetValue(LevelProperty, Math.Clamp(value, 1, 55));
    }

    public static readonly DependencyProperty AgeDaysProperty =
        DependencyProperty.Register(nameof(AgeDays), typeof(double), typeof(PlantView),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double AgeDays
    {
        get => (double)GetValue(AgeDaysProperty);
        set => SetValue(AgeDaysProperty, Math.Clamp(value, 1d, 365d));
    }

    public PlantView()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += (_, _) =>
        {
            _phase += .03;
            if (_wither > 0) _wither = Math.Min(1, _wither + .014);
            InvalidateVisual();
        };
        _timer.Start();
    }

    public void PlayWither()
    {
        _wither = .01;
        InvalidateVisual();
    }

    public void ResetAfterFailure()
    {
        _wither = 0;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (ActualWidth < 20 || ActualHeight < 20) return;

        var cx = ActualWidth / 2;
        var groundY = ActualHeight * .82;
        var progress = VisualProgress(AgeDays);
        DrawAmbient(dc, cx, groundY, progress);

        if (progress < .50)
            DrawPottedPlant(dc, cx, groundY, progress);
        else
            DrawRootedTree(dc, cx, groundY, progress);

        if (_wither > .08) DrawFallingPieces(dc, cx, groundY, progress);
    }

    private void DrawAmbient(DrawingContext dc, double cx, double groundY, double progress)
    {
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(28, 31, 62, 44)), null,
            new Point(cx, groundY + 26), ActualWidth * (.15 + progress * .11), ActualHeight * .025);

        if (progress > .42 && _wither < .65)
        {
            for (var i = 0; i < Math.Min(9, Level / 6); i++)
            {
                var angle = _phase * .16 + i * 2.17;
                var radius = 75 + (i % 3) * 24;
                var x = cx + Math.Cos(angle) * radius;
                var y = groundY - ActualHeight * (.25 + progress * .28) + Math.Sin(angle * 1.7) * 28;
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(90, 247, 196, 104)), null,
                    new Point(x, y), 2.5, 2.5);
            }
        }
    }

    private void DrawPottedPlant(DrawingContext dc, double cx, double groundY, double progress)
    {
        var potFade = progress > .45 ? Math.Max(0, 1 - (progress - .45) / .05) : 1;
        DrawPot(dc, cx, groundY, potFade);
        var local = Math.Min(1, progress / .5);
        var height = ActualHeight * (.11 + local * .49);
        var top = groundY - height;
        var sway = Math.Sin(_phase) * (3.5 + local * 4) * (1 - _wither * .6);
        var stemWidth = 3.5 + local * 10;
        DrawStem(dc, cx, groundY, top, sway, stemWidth, local);

        if (AgeDays < 3)
        {
            var opening = 20 + (AgeDays - 1) * 4;
            DrawLeaf(dc, new Point(cx + sway - opening, top + 13), 20 + AgeDays * 1.7, 12 + AgeDays,
                -24 + Math.Sin(_phase * .78) * 2.5, 0);
            DrawLeaf(dc, new Point(cx + sway + opening * .92, top + 10), 19 + AgeDays * 1.9, 11 + AgeDays,
                29 + Math.Sin(_phase * .91 + 1.2) * 3, 1);
            return;
        }

        var leafCount = Math.Clamp(2 + (int)(local * 13.5), 2, 15);
        for (var i = 0; i < leafCount; i++)
        {
            var evenSpread = i / (double)Math.Max(1, leafCount - 1);
            var t = Math.Clamp(.14 + evenSpread * .72 + SignedNoise(i, 11) * .035, .12, .90);
            var side = OrganicSide(i);
            var branchX = cx + sway * t;
            var branchY = groundY - height * t;
            var emergence = Math.Max(0, (i - 1.7) / 13.5);
            var maturity = SmoothStep(Math.Clamp((local - emergence) / .09, 0, 1));
            var sizeScale = .20 + maturity * .80;
            var reach = (22 + local * 37 + Noise(i, 23) * 18) * (.45 + maturity * .55);
            var rise = SignedNoise(i, 37) * (12 + local * 13);
            var leafPoint = new Point(branchX + side * reach, branchY + rise + _wither * 22);
            DrawBranch(dc, new Point(branchX, branchY), leafPoint,
                (1.2 + local * 1.8) * (.55 + maturity * .45));
            var leafSway = Math.Sin(_phase * (.72 + Noise(i, 51) * .35) + Noise(i, 61) * 6.28)
                * (2.2 + Noise(i, 71) * 4.2);
            DrawLeaf(dc, leafPoint,
                (18 + local * 14 + Noise(i, 83) * 8) * sizeScale,
                (10 + local * 6 + Noise(i, 97) * 5) * sizeScale,
                side * (18 + Noise(i, 109) * 24) + leafSway + _wither * side * 72, i);
        }

        if (progress >= .30)
        {
            var flowers = Math.Clamp(1 + (int)((progress - .30) / .20 * 8), 1, 9);
            for (var i = 0; i < flowers; i++)
            {
                var t = .32 + Noise(i, 131) * .55;
                var side = OrganicSide(i + 4);
                DrawFlower(dc,
                    new Point(cx + sway * t + side * (34 + Noise(i, 149) * 58),
                        groundY - height * t + SignedNoise(i, 157) * 13 + _wither * 30),
                    7 + local * 3 + Noise(i, 163) * 2, i);
            }
        }
    }

    private void DrawRootedTree(DrawingContext dc, double cx, double groundY, double progress)
    {
        var tree = Math.Clamp((progress - .50) / .50, 0, 1);
        DrawGround(dc, cx, groundY, tree);
        DrawRoots(dc, cx, groundY, tree);

        var height = ActualHeight * (.43 + tree * .32);
        var topY = groundY - height;
        var sway = Math.Sin(_phase) * (3.2 - tree * 1.4) * (1 - _wither * .5);
        var trunkWidth = 10 + tree * 30;
        DrawTrunk(dc, cx, groundY, topY, sway, trunkWidth, tree);

        var branchCount = 3 + (int)(tree * 8);
        var tips = new List<Point>();
        for (var i = 0; i < branchCount; i++)
        {
            var evenSpread = i / (double)Math.Max(1, branchCount - 1);
            var level = Math.Clamp(.25 + evenSpread * .61 + SignedNoise(i, 181) * .045, .22, .90);
            var side = OrganicSide(i + 2);
            var start = new Point(cx + sway * level * .5, groundY - height * level);
            var reach = 44 + tree * 77 + Noise(i, 193) * 45;
            var end = new Point(start.X + side * reach,
                start.Y - 12 - tree * 28 - Noise(i, 211) * 38 + _wither * 35);
            DrawBranch(dc, start, end, Math.Max(3, trunkWidth * (1 - level) * .34));
            tips.Add(end);

            var twigs = tree > .2 ? 2 + (int)(tree * 2) : 1;
            for (var j = 0; j < twigs; j++)
            {
                var twigSide = j == 0 ? side : (Noise(i * 5 + j, 223) > .42 ? side : -side);
                var twigEnd = new Point(
                    end.X + twigSide * (16 + j * 12 + Noise(i + j, 227) * 18),
                    end.Y + SignedNoise(i * 7 + j, 239) * 29 + _wither * 18);
                DrawBranch(dc, end, twigEnd, 2 + tree * 2);
                tips.Add(twigEnd);
            }
        }

        var leafClusters = Math.Min(tips.Count, 6 + (int)(tree * 18));
        for (var i = 0; i < leafClusters; i++)
        {
            var tip = tips[i % tips.Count];
            var angle = Noise(i, 251) * Math.PI * 2;
            var offset = new Vector(Math.Cos(angle) * (9 + tree * 18), Math.Sin(angle) * (7 + tree * 15));
            DrawLeaf(dc, tip + offset, 25 + tree * 16, 15 + tree * 8,
                SignedNoise(i, 263) * 42 + Math.Sin(_phase * (.65 + Noise(i, 269) * .3) + i) * 4 + _wither * 65, i);
        }

        if (tree > .80 && _wither < .80)
        {
            var fruits = 1 + (int)((tree - .80) / .20 * 9);
            for (var i = 0; i < fruits; i++)
            {
                var tip = tips[(i * 3 + 1) % tips.Count];
                var bob = Math.Sin(_phase + i) * 2;
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(225, 101, 74)), null,
                    new Point(tip.X + (i % 2 == 0 ? 10 : -10), tip.Y + 22 + bob), 8 + tree * 3, 9 + tree * 3);
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(72, 104, 63)), 2),
                    new Point(tip.X, tip.Y + 4), new Point(tip.X + (i % 2 == 0 ? 10 : -10), tip.Y + 15 + bob));
            }
        }
    }

    private void DrawPot(DrawingContext dc, double cx, double y, double opacity)
    {
        var potBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * opacity), 218, 122, 79));
        var lipBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * opacity), 235, 148, 99));
        var pot = new StreamGeometry();
        using (var g = pot.Open())
        {
            g.BeginFigure(new Point(cx - 58, y - 6), true, true);
            g.LineTo(new Point(cx + 58, y - 6), true, false);
            g.LineTo(new Point(cx + 42, y + 66), true, false);
            g.QuadraticBezierTo(new Point(cx, y + 79), new Point(cx - 42, y + 66), true, false);
        }
        dc.DrawGeometry(potBrush, null, pot);
        dc.DrawRoundedRectangle(lipBrush, null, new Rect(cx - 66, y - 14, 132, 24), 8, 8);
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(220 * opacity), 91, 68, 47)), null,
            new Point(cx, y - 5), 51, 7);
    }

    private void DrawStem(DrawingContext dc, double cx, double bottom, double top, double sway, double width, double growth)
    {
        var pen = new Pen(PlantBrush(.92), width)
        { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var figure = new PathFigure(new Point(cx, bottom),
        [
            new BezierSegment(
                new Point(cx - sway * .2, bottom - (bottom - top) * .36),
                new Point(cx + sway * .7, top + (bottom - top) * .25),
                new Point(cx + sway, top + _wither * 24), true)
        ], false);
        dc.DrawGeometry(null, pen, new PathGeometry([figure]));
    }

    private void DrawTrunk(DrawingContext dc, double cx, double bottom, double top, double sway, double width, double growth)
    {
        var trunk = new StreamGeometry();
        using var g = trunk.Open();
        g.BeginFigure(new Point(cx - width / 2, bottom), true, true);
        g.BezierTo(new Point(cx - width * .42, bottom - (bottom - top) * .40),
            new Point(cx - width * .18 + sway, top + 70), new Point(cx - width * .12 + sway, top), true, false);
        g.LineTo(new Point(cx + width * .12 + sway, top), true, false);
        g.BezierTo(new Point(cx + width * .24 + sway, top + 70),
            new Point(cx + width * .44, bottom - (bottom - top) * .40), new Point(cx + width / 2, bottom), true, false);
        dc.DrawGeometry(new LinearGradientBrush(
            Color.FromRgb(104, 80, 52), Color.FromRgb(137, 102, 62), 0), null, trunk);
        dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(65, 52, 45, 31)), 2),
            new Point(cx - width * .1, bottom - 12), new Point(cx - width * .04 + sway, top + 30));
    }

    private void DrawGround(DrawingContext dc, double cx, double y, double growth)
    {
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(221, 218, 187)), null,
            new Point(cx, y + 20), 90 + growth * 100, 24 + growth * 8);
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(105, 132, 83)), null,
            new Point(cx, y + 11), 80 + growth * 92, 15 + growth * 7);
    }

    private void DrawRoots(DrawingContext dc, double cx, double y, double growth)
    {
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(125, 105, 77, 49)), 3 + growth * 4);
        for (var i = 0; i < 7; i++)
        {
            var side = i % 2 == 0 ? -1 : 1;
            var reach = 35 + i * 12 + growth * 30;
            var figure = new PathFigure(new Point(cx, y + 5),
            [
                new BezierSegment(new Point(cx + side * reach * .25, y + 15),
                    new Point(cx + side * reach * .65, y + 18 + i * 2),
                    new Point(cx + side * reach, y + 12 + i * 3), true)
            ], false);
            dc.DrawGeometry(null, pen, new PathGeometry([figure]));
        }
    }

    private void DrawBranch(DrawingContext dc, Point start, Point end, double width)
    {
        dc.DrawLine(new Pen(PlantBrush(.83), width)
        { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, start, end);
    }

    private void DrawLeaf(DrawingContext dc, Point center, double rx, double ry, double angle, int index)
    {
        var fall = _wither > .32 && index % 3 == 0 ? (_wither - .32) * ActualHeight * .38 : 0;
        center = new Point(center.X + Math.Sin(_phase * 1.5 + index) * fall * .08, center.Y + fall);
        dc.PushTransform(new RotateTransform(angle, center.X, center.Y));
        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            g.BeginFigure(new Point(center.X - rx, center.Y), true, true);
            g.BezierTo(new Point(center.X - rx * .32, center.Y - ry),
                new Point(center.X + rx * .68, center.Y - ry), new Point(center.X + rx, center.Y), true, false);
            g.BezierTo(new Point(center.X + rx * .42, center.Y + ry),
                new Point(center.X - rx * .52, center.Y + ry * .72), new Point(center.X - rx, center.Y), true, false);
        }
        dc.DrawGeometry(LeafBrush(index), null, geometry);
        dc.Pop();
    }

    private void DrawFlower(DrawingContext dc, Point center, double size, int index)
    {
        if (_wither > .48 && index % 2 == 0) center.Y += (_wither - .48) * ActualHeight * .30;
        for (var i = 0; i < 6; i++)
        {
            var angle = i * Math.PI / 3 + _phase * .05;
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(244, 157, 155)), null,
                new Point(center.X + Math.Cos(angle) * size, center.Y + Math.Sin(angle) * size),
                size * .70, size);
        }
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(248, 199, 91)), null, center, size * .55, size * .55);
    }

    private void DrawFallingPieces(DrawingContext dc, double cx, double groundY, double progress)
    {
        var count = Math.Min(9, 2 + Level / 7);
        for (var i = 0; i < count; i++)
        {
            var fall = Math.Max(0, _wither - i * .06);
            if (fall <= 0) continue;
            var x = cx + Math.Sin(_phase * 1.8 + i * 1.7) * (40 + i * 9);
            var y = groundY - ActualHeight * (.50 - (i % 4) * .08) + fall * ActualHeight * .58;
            DrawLeaf(dc, new Point(x, Math.Min(groundY + 8, y)), 10, 6, fall * 240 + i * 23, i + 20);
        }
    }

    private Brush PlantBrush(double strength)
    {
        var healthy = Color.FromRgb(45, (byte)(111 + strength * 20), 76);
        var dry = Color.FromRgb(115, 91, 60);
        return new SolidColorBrush(ColorExtensions.Lerp(healthy, dry, (float)_wither));
    }

    private Brush LeafBrush(int index)
    {
        var options = new[]
        {
            Color.FromRgb(65, 139, 88),
            Color.FromRgb(82, 153, 96),
            Color.FromRgb(101, 164, 107),
            Color.FromRgb(57, 126, 83)
        };
        return new SolidColorBrush(ColorExtensions.Lerp(options[index % options.Length],
            Color.FromRgb(135, 103, 68), (float)_wither));
    }

    private static double VisualProgress(double ageDays)
    {
        var points = new (double Day, double Progress)[]
        {
            (1, .018), (2, .026), (3, .036), (7, .070), (14, .115),
            (30, .20), (60, .32), (90, .42), (150, .52),
            (210, .68), (280, .83), (365, 1)
        };
        if (ageDays <= points[0].Day) return points[0].Progress;
        for (var i = 1; i < points.Length; i++)
        {
            if (ageDays > points[i].Day) continue;
            var previous = points[i - 1];
            var current = points[i];
            var t = (ageDays - previous.Day) / (current.Day - previous.Day);
            t = t * t * (3 - 2 * t);
            return previous.Progress + (current.Progress - previous.Progress) * t;
        }
        return 1;
    }

    private static int OrganicSide(int index)
    {
        // A deterministic irregular sequence keeps the silhouette balanced without looking mirrored.
        int[] sequence = [-1, 1, 1, -1, 1, -1, -1, 1, -1, 1, 1, -1, -1, 1, 1];
        return sequence[index % sequence.Length];
    }

    private static double Noise(int index, int salt)
    {
        unchecked
        {
            var value = (uint)(index * 374761393 + salt * 668265263);
            value = (value ^ (value >> 13)) * 1274126177;
            value ^= value >> 16;
            return (value & 0x00FFFFFF) / 16777215d;
        }
    }

    private static double SignedNoise(int index, int salt) => Noise(index, salt) * 2 - 1;

    private static double SmoothStep(double value) => value * value * (3 - 2 * value);
}

internal static class ColorExtensions
{
    public static Color Lerp(Color a, Color b, float t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));
}
