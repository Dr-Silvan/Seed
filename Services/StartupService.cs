using System.Diagnostics;
using Microsoft.Win32;

namespace Seed.Services;

public static class StartupService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Seed";

    public static void EnsureEnabled()
    {
        try
        {
            var exe = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exe)) return;
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            key?.SetValue(ValueName, $"\"{exe}\" --background");
        }
        catch
        {
            // 회사 정책 등으로 시작 프로그램 등록이 막혀도 앱 자체는 정상 실행합니다.
        }
    }
}
