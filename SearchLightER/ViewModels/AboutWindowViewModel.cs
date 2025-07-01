using Avalonia.Controls;
using Epoxy;
using naget.Models.SearchEngine;
using naget.Views;
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

	public AboutWindowViewModel()
	{
		// ウィンドウがロードされた時の処理
		WindowWell.Add(Window.LoadedEvent, () =>
		{
			Debug.WriteLine("AboutWindow Loaded");
			AvaloniaVersion = App.GetLibraryInfo("Avalonia")?["PackageVersion"].ToString() ?? "Unknown";
			WebViewVersion = App.GetLibraryInfo("WebViewControl-Avalonia")?["PackageVersion"].ToString() ?? "Unknown";
			return default;
		});
	}
}
