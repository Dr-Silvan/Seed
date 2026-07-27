using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Seed;

public partial class App : Application
{
    private Forms.NotifyIcon? _tray;
    private MainWindow? _main;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        Services.StartupService.EnsureEnabled();
        var background = e.Args.Any(a => a.Equals("--background", StringComparison.OrdinalIgnoreCase));
        _main = new MainWindow(background);
        MainWindow = _main;
        CreateTray();
        _main.Show();
    }

    private void CreateTray()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Seed 열기", null, (_, _) => ShowMain());
        menu.Items.Add("식물 위젯 켜기", null, (_, _) => _main?.EnableWidget());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("완전 종료", null, (_, _) => ExitSeed());

        _tray = new Forms.NotifyIcon
        {
            Text = "Seed — 오늘도 자라고 있어요",
            Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? "") ?? SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        _tray.DoubleClick += (_, _) => ShowMain();
    }

    public void ShowMain()
    {
        if (_main is null) return;
        _main.Show();
        if (_main.WindowState == WindowState.Minimized) _main.WindowState = WindowState.Normal;
        _main.Activate();
    }

    public void ShowBackgroundNotice()
    {
        _tray?.ShowBalloonTip(2500, "Seed는 계속 자라고 있어요",
            "식물 위젯과 기록은 백그라운드에서 계속 실행됩니다. 완전히 종료하려면 트레이 아이콘을 우클릭하세요.",
            Forms.ToolTipIcon.Info);
    }

    public void ExitSeed()
    {
        _main?.PrepareForExit();
        _tray?.Dispose();
        _tray = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        base.OnExit(e);
    }
}
