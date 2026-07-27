using System.Windows;
using System.Windows.Input;
using Seed.Models;
using Seed.Services;

namespace Seed;

public partial class WidgetWindow : Window
{
    public WidgetWindow(AppState state)
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var area = SystemParameters.WorkArea;
            Left = area.Left + 18;
            Top = area.Bottom - Height - 18;
            Update(state);
        };
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        };
    }

    public void Update(AppState state)
    {
        var days = Math.Max(0, (int)(DateTime.Now - state.StartedAt).TotalDays);
        var stage = SeedContent.StageFor(days);
        Plant.Level = stage.Level;
        DayText.Text = $"DAY {days + 1}";
        LevelText.Text = $"Lv.{stage.Level} · {stage.Name}";
    }

    private void CloseWidget(object sender, RoutedEventArgs e) => Close();
}
