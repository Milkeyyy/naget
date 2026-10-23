using Avalonia.Controls;
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
			WebViewVersion = App.GetLibraryInfo("Avalonia.Controls.WebView")?.PackageVersion ?? "Unknown";
			return default;
		});
	}
}
