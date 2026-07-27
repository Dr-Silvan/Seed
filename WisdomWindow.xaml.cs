using System.Windows;
using Seed.Services;

namespace Seed;

public partial class WisdomWindow : Window
{
    private readonly WisdomEntry _entry;

    public WisdomWindow(WisdomEntry entry)
    {
        InitializeComponent();
        _entry = entry;
        QuoteText.Text = entry.Text;
        AttributionText.Text = $"— {entry.Attribution} · {entry.Reference}";
    }

    private void OpenWeb(object sender, RoutedEventArgs e)
    {
        var query = Uri.EscapeDataString($"{_entry.SearchTerms} 해설");
        MainWindow.OpenUrl($"https://www.google.com/search?q={query}");
    }

    private void OpenVideo(object sender, RoutedEventArgs e)
    {
        var query = Uri.EscapeDataString($"{_entry.SearchTerms} 철학 강의");
        MainWindow.OpenUrl($"https://www.youtube.com/results?search_query={query}");
    }
}
