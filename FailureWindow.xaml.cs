using System.Windows;
using System.Windows.Controls;
using Seed.Services;

namespace Seed;

public partial class FailureWindow : Window
{
    public string SelectedReason { get; private set; } = "";
    public string Note => NoteBox.Text.Trim();

    public FailureWindow()
    {
        InitializeComponent();
        foreach (var reason in SeedContent.Reasons)
        {
            var button = new RadioButton
            {
                Content = reason,
                GroupName = "reason",
                Margin = new Thickness(0, 0, 8, 8),
                Padding = new Thickness(12, 8, 12, 8)
            };
            button.Checked += (_, _) => SelectedReason = reason;
            ReasonPanel.Children.Add(button);
        }
    }

    private void Confirm(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedReason))
        {
            MessageBox.Show("가장 가까운 원인 하나를 선택해주세요.");
            return;
        }
        DialogResult = true;
    }

    private void Cancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
