using Avalonia.Controls;
using Avalonia.Platform;
using Epoxy;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace naget.ViewModels;

[ViewModel]
public class AboutWindowViewModel
{
	public Well<Window> WindowWell { get; } = Well.Factory.Create<Window>();

	public static string WindowTitle => App.ProductName;
	
	public static string TitleText => App.ProductName;
	public static string VersionText => "Version " + App.ProductFullVersion;

	public static string OSVersion => $"{RuntimeInformation.OSDescription} ({RuntimeInformation.RuntimeIdentifier})";
	public static string NETCLRVersion => Environment.Version.ToString();
	public string AvaloniaVersion { get; set; } = "Unknown";
	public string WebViewVersion { get; set; } = "Unknown";

	public Command OpenLicensesCommand { get; }

	public AboutWindowViewModel()
	{
		OpenLicensesCommand = Command.Factory.Create(() =>
		{
			App.LicenseWindow!.ShowDialog(App.AboutWindow!);
			return default;
		});

		// ウィンドウがロードされた時の処理
		WindowWell.Add(Window.LoadedEvent, () =>
		{
			App.Logger.Debug("AboutWindow Loaded");
			AvaloniaVersion = App.GetLibraryInfo("Avalonia")?.PackageVersion ?? "Unknown";

			string packageVersion = App.GetLibraryInfo("Avalonia.Controls.WebView")?.PackageVersion ?? "Unknown";
			string? engineVersion = GetWebViewEngineVersion();
			WebViewVersion = engineVersion is null ? packageVersion : $"{packageVersion} ({engineVersion})";
			return default;
		});
	}

	private static string? GetWebViewEngineVersion()
	{
		if (OperatingSystem.IsWindows())
		{
			DetailedWebViewAdapterInfo info = WebViewAdapterInfo.GetAdapterInfo(WebViewAdapterType.WebView2);
			return info.IsInstalled && !string.IsNullOrEmpty(info.Version) ? $"WebView2 {info.Version}" : null;
		}

		if (OperatingSystem.IsMacOS())
		{
			string? safari = ReadPlistValue("/Applications/Safari.app/Contents/Info.plist", "CFBundleShortVersionString");
			string? webKit = ReadPlistValue("/System/Library/Frameworks/WebKit.framework/Versions/A/Resources/Info.plist", "CFBundleVersion");

			if (safari is not null && webKit is not null) return $"Safari {safari} / WebKit {webKit}";
			if (safari is not null) return $"Safari {safari}";
			if (webKit is not null) return $"WebKit {webKit}";
		}

		return null;
	}

	private static string? ReadPlistValue(string path, string key)
	{
		try
		{
			using Process process = Process.Start(new ProcessStartInfo("/usr/bin/plutil")
			{
				ArgumentList = { "-extract", key, "raw", "-o", "-", path },
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
			})!;

			string value = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();
			return process.ExitCode == 0 && value.Length > 0 ? value : null;
		}
		catch (Exception ex)
		{
			App.Logger.Warn($"Failed to read {key} from {path}: {ex.Message}");
			return null;
		}
	}
}
