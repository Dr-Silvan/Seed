using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Seed.Models;
using Seed.Services;

namespace Seed;

public partial class MainWindow : Window
{
    private readonly SeedStore _store = new();
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };
    private AppState _state;
    private WidgetWindow? _widget;
    private readonly bool _startHidden;
    private bool _allowExit;
    private WisdomEntry _wisdom = DailyWisdom.RandomEntry();
    private DateTime _wisdomDate = DateTime.Today;

    public MainWindow(bool startHidden = false)
    {
        InitializeComponent();
        _startHidden = startHidden;
        _state = _store.Load();
        HabitNameBox.Text = _state.HabitName;
        _clock.Tick += (_, _) => RefreshDashboard();
        _clock.Start();
        Loaded += (_, _) =>
        {
            if (!_state.BackgroundModeConfigured)
            {
                _state.WidgetEnabled = true;
                _state.BackgroundModeConfigured = true;
            }
            if (_startHidden) _state.WidgetEnabled = true;
            RefreshAll();
            if (_state.WidgetEnabled) ShowWidget();
            if (_startHidden) Hide();
            _store.Save(_state);
        };
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_allowExit && _state.WidgetEnabled)
        {
            e.Cancel = true;
            Hide();
            (Application.Current as App)?.ShowBackgroundNotice();
            return;
        }
        _widget?.Close();
        base.OnClosing(e);
    }

    public void PrepareForExit()
    {
        _allowExit = true;
        _widget?.Close();
    }

    private int CurrentDays => Math.Max(0, (int)(DateTime.Now - _state.StartedAt).TotalDays);

    private void RefreshAll()
    {
        RefreshDashboard();
        BuildCalendar();
        RefreshInsights();
        StartedAtText.Text = _state.StartedAt.ToString("yyyy년 M월 d일 HH:mm");
        StartDatePicker.SelectedDate = _state.StartedAt.Date;
        HabitNameBox.Text = _state.HabitName;
    }

    private void RefreshDashboard()
    {
        var elapsed = DateTime.Now - _state.StartedAt;
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        var days = (int)elapsed.TotalDays;
        var stage = SeedContent.StageFor(days);
        MainPlant.Level = stage.Level;
        MainPlant.AgeDays = Math.Min(365, elapsed.TotalDays + 1);
        DayText.Text = $"DAY {days + 1}";
        PreciseTimeText.Text = $"{days}일 {elapsed.Hours:D2}시간 {elapsed.Minutes:D2}분 {elapsed.Seconds:D2}초";
        HabitText.Text = $"{_state.HabitName} · {_state.StartedAt:yyyy.MM.dd} 시작";
        StageText.Text = $"Lv.{stage.Level}  {stage.Name}";
        StageMessage.Text = stage.Message;
        if (_wisdomDate != DateTime.Today)
        {
            _wisdom = DailyWisdom.RandomEntry(_wisdom);
            _wisdomDate = DateTime.Today;
        }
        DailyMessage.Text = _wisdom.Text;
        DailyAttribution.Text = $"— {_wisdom.Attribution} · {_wisdom.Reference}";

        var index = Array.IndexOf(SeedContent.Stages, stage);
        if (index < SeedContent.Stages.Length - 1)
        {
            var next = SeedContent.Stages[index + 1];
            NextStageText.Text = $"{next.Name}까지 {Math.Max(0, next.MinimumDays - days)}일";
            GrowthProgress.Minimum = stage.MinimumDays;
            GrowthProgress.Maximum = next.MinimumDays;
            GrowthProgress.Value = days;
        }
        else
        {
            NextStageText.Text = "생명의 나무가 함께하고 있어요";
            GrowthProgress.Minimum = 0;
            GrowthProgress.Maximum = 1;
            GrowthProgress.Value = 1;
        }
        _widget?.Update(_state);
    }

    private void Navigate(object sender, RoutedEventArgs e)
    {
        if (TodayPage is null) return;
        TodayPage.Visibility = TodayNav.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        CalendarPage.Visibility = CalendarNav.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        InsightsPage.Visibility = InsightsNav.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = SettingsNav.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        if (CalendarNav.IsChecked == true) BuildCalendar();
        if (InsightsNav.IsChecked == true) RefreshInsights();
    }

    private void OpenTemptation(object sender, RoutedEventArgs e)
    {
        var dialog = new TemptationWindow(_state) { Owner = this };
        dialog.ShowDialog();
        if (dialog.DidSave)
        {
            _store.Save(_state);
            RefreshAll();
        }
    }

    private void OpenWisdom(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        new WisdomWindow(_wisdom) { Owner = this }.ShowDialog();
    }

    private async void OpenFailure(object sender, RoutedEventArgs e)
    {
        var dialog = new FailureWindow() { Owner = this };
        if (dialog.ShowDialog() != true) return;

        MainPlant.PlayWither();
        _widget?.PlayWither();
        await Task.Delay(2300);
        _state.Attempts.Add(new AttemptRecord
        {
            StartedAt = _state.StartedAt,
            EndedAt = DateTime.Now,
            Reason = dialog.SelectedReason,
            Note = dialog.Note
        });
        _state.StartedAt = DateTime.Now;
        MainPlant.ResetAfterFailure();
        _store.Save(_state);
        RefreshAll();
        MessageBox.Show("기록했어요. 식물이 죽은 것이 아니라, 다음 씨앗을 위한 정보가 남았어요.",
            "다시 심기", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BuildCalendar()
    {
        CalendarGrid.Children.Clear();
        var start = _state.StartedAt.Date;
        var yearStart = start;
        for (var i = 0; i < 365; i++)
        {
            var date = yearStart.AddDays(i);
            var isCurrent = date >= _state.StartedAt.Date && date <= DateTime.Today;
            var failed = _state.Attempts.Any(a => date >= a.StartedAt.Date && date <= a.EndedAt.Date);
            var future = date > DateTime.Today;
            var cell = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(3),
                Background = new SolidColorBrush(
                    future ? Color.FromRgb(235, 234, 225) :
                    isCurrent ? Color.FromRgb(82, 145, 105) :
                    failed ? Color.FromRgb(211, 151, 105) :
                    Color.FromRgb(222, 226, 216)),
                ToolTip = $"{date:yyyy.MM.dd}" + (isCurrent ? " · 현재 시도" : failed ? " · 지난 시도" : "")
            };
            CalendarGrid.Children.Add(cell);
        }
    }

    private void RefreshInsights()
    {
        AttemptList.Items.Clear();
        var reasonGroup = _state.Attempts
            .GroupBy(a => a.Reason)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        var topReason = reasonGroup?.Key ?? "아직 기록 없음";
        TopReasonText.Text = reasonGroup is null ? topReason : $"{topReason} · {reasonGroup.Count()}회";
        AdviceText.Text = reasonGroup is null
            ? "실패를 기록하면 반복되는 원인과 다음 행동을 이곳에서 제안해드려요."
            : SeedContent.AdviceFor(topReason);

        var best = _state.Attempts
            .Select(a => (a.EndedAt - a.StartedAt).TotalDays)
            .Append((DateTime.Now - _state.StartedAt).TotalDays)
            .DefaultIfEmpty(0).Max();
        var overcome = _state.Temptations.Count(t => t.Overcame);
        StatsText.Text = $"최고 기록  {Math.Floor(best)}일\n완료한 시도  {_state.Attempts.Count}회\n이겨낸 충동  {overcome}회";

        foreach (var attempt in _state.Attempts.OrderByDescending(a => a.EndedAt))
        {
            AttemptList.Items.Add(new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(232, 231, 221)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 12, 0, 12),
                Child = new TextBlock
                {
                    Text = $"{attempt.StartedAt:yyyy.MM.dd} → {attempt.EndedAt:yyyy.MM.dd}  ·  {attempt.Reason}\n{attempt.Note}",
                    Foreground = new SolidColorBrush(Color.FromRgb(64, 82, 73)),
                    TextWrapping = TextWrapping.Wrap
                }
            });
        }
    }

    private void SaveSettings(object sender, RoutedEventArgs e)
    {
        _state.HabitName = string.IsNullOrWhiteSpace(HabitNameBox.Text) ? "나의 목표" : HabitNameBox.Text.Trim();
        if (StartDatePicker.SelectedDate is DateTime selected)
        {
            var chosen = selected.Date + _state.StartedAt.TimeOfDay;
            _state.StartedAt = chosen > DateTime.Now ? DateTime.Now : chosen;
        }
        _store.Save(_state);
        RefreshAll();
        MessageBox.Show("저장했어요.", "Seed");
    }

    private void BackupAndReset(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(
            "현재 연속 기록과 실패·충동 기록을 백업한 뒤 모두 초기화합니다.\n\n이 작업을 계속할까요?",
            "Seed 기록 초기화",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;

        try
        {
            var backupPath = _store.CreateBackup(_state);
            _state.Attempts.Clear();
            _state.Temptations.Clear();
            _state.StartedAt = DateTime.Now;
            MainPlant.ResetAfterFailure();
            _store.Save(_state);
            RefreshAll();
            MessageBox.Show(
                $"기록을 백업하고 새 씨앗으로 시작했습니다.\n\n백업 위치:\n{backupPath}",
                "초기화 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"백업 파일을 만들지 못해 초기화하지 않았습니다.\n\n{exception.Message}",
                "초기화 취소",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ToggleWidget(object sender, RoutedEventArgs e)
    {
        if (_widget is null)
        {
            ShowWidget();
            _state.WidgetEnabled = true;
        }
        else
        {
            _widget.Close();
            _widget = null;
            _state.WidgetEnabled = false;
        }
        _store.Save(_state);
    }

    public void EnableWidget()
    {
        if (_widget is null) ShowWidget();
        _state.WidgetEnabled = true;
        _store.Save(_state);
    }

    private void ShowWidget()
    {
        _widget = new WidgetWindow(_state);
        _widget.Closed += (_, _) =>
        {
            _widget = null;
            _state.WidgetEnabled = false;
            _store.Save(_state);
        };
        _widget.Show();
    }

    public static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { MessageBox.Show("링크를 열 수 없습니다."); }
    }
}
