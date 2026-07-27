using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Seed.Models;
using Seed.Services;

namespace Seed;

public partial class TemptationWindow : Window
{
    private readonly AppState _state;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private int _remaining = 60;
    private int _cycleSecond;
    public bool DidSave { get; private set; }

    public TemptationWindow(AppState state)
    {
        InitializeComponent();
        _state = state;
        GuideMessage.Text = SeedContent.GroundingMessages[Random.Shared.Next(SeedContent.GroundingMessages.Length)];
        TriggerBox.ItemsSource = SeedContent.Reasons;
        TriggerBox.SelectedIndex = 0;
        TriggerBox.SelectionChanged += (_, _) =>
            ActionText.Text = SeedContent.AdviceFor(TriggerBox.SelectedItem?.ToString() ?? "");
        ActionText.Text = SeedContent.AdviceFor(SeedContent.Reasons[0]);
        BuildResources();
        _timer.Tick += TimerTick;
    }

    private void BuildResources()
    {
        foreach (var item in SeedContent.Resources)
        {
            var button = new Button
            {
                Content = $"{item.Kind} · {item.Title}  ↗",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 7),
                Padding = new Thickness(10, 8, 10, 8)
            };
            button.Click += (_, _) => MainWindow.OpenUrl(item.Url);
            ResourceList.Items.Add(button);
        }
    }

    private void StartBreathing(object sender, RoutedEventArgs e)
    {
        _remaining = 60;
        _cycleSecond = 0;
        StartButton.IsEnabled = false;
        _timer.Start();
        AnimateBreath(true);
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        _remaining--;
        _cycleSecond = (_cycleSecond + 1) % 10;
        CountdownText.Text = _remaining.ToString();

        if (_cycleSecond == 0)
        {
            BreathText.Text = "들이마셔요";
            AnimateBreath(true);
        }
        else if (_cycleSecond == 4)
        {
            BreathText.Text = "내쉬어요";
            AnimateBreath(false);
        }

        if (_remaining <= 0)
        {
            _timer.Stop();
            BreathText.Text = "잘했어요";
            CountdownText.Text = "한 번 더 필요하면 다시 시작해요";
            StartButton.IsEnabled = true;
            StartButton.Content = "한 번 더";
        }
    }

    private void AnimateBreath(bool inhale)
    {
        var from = inhale ? 100 : 180;
        var to = inhale ? 180 : 100;
        var seconds = inhale ? 4 : 6;
        var animation = new DoubleAnimation(from, to, TimeSpan.FromSeconds(seconds))
        { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } };
        BreathCircle.BeginAnimation(WidthProperty, animation);
        BreathCircle.BeginAnimation(HeightProperty, animation);
    }

    private void Save(bool overcame)
    {
        _state.Temptations.Add(new TemptationRecord
        {
            At = DateTime.Now,
            Trigger = TriggerBox.SelectedItem?.ToString() ?? "기타",
            Overcame = overcame
        });
        DidSave = true;
        DialogResult = true;
    }

    private void SaveOvercame(object sender, RoutedEventArgs e) => Save(true);
    private void SaveNotYet(object sender, RoutedEventArgs e) => Save(false);
}
