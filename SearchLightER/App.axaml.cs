using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SearchLight.Models;
using SearchLight.ViewModels;
using SearchLight.Views;
using SharpHook.Native;
using System;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;

namespace SearchLight;

public partial class App : Application
{
	private static string _name = string.Empty;
	public static string ProductName => _name;

	private static string _version = string.Empty;
	public static string ProductVersion => _version;

	/// <summary>
	/// コンフィグ等のファイルを保存するフォルダー
	/// </summary>
	public static string ConfigFolder => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);

	public static Window MainWindow;
	public static Window SettingsWindow;
	public static Window BrowserWindow;

	public override void Initialize()
	{
		var a = Assembly.GetExecutingAssembly().GetName();
		if (a.Name != null) _name = a.Name;
		if (a.Version != null) _version = a.Version.ToString();
		AvaloniaXamlLoader.Load(this);
	}

	public override async void OnFrameworkInitializationCompleted()
	{
		Assets.Locales.Resources.Culture = new CultureInfo("ja-JP");
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			DataContext = new AppViewModel(); // 通知領域メニューのためのビューモデル
			desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			// フォルダーを作成する
			Directory.CreateDirectory(ConfigFolder);

			// 検索エンジンのリストを読み込む
			SearchEngineManager.Load();

			MainWindow = new MainWindow();
			SettingsWindow = new SettingsWindow();
			BrowserWindow = new BrowserWindow();

			desktop.MainWindow = SettingsWindow;

			// ホットキーの登録
			var hotKeyManager = new SearchLight.Models.HotKeyManager();
			hotKeyManager.Register(new HotKeyGroup([KeyCode.VcLeftControl, KeyCode.VcLeftAlt, KeyCode.VcA], MainWindow.Show));
			hotKeyManager.Register(new HotKeyGroup([KeyCode.VcLeftControl, KeyCode.VcLeftAlt, KeyCode.VcQ], SettingsWindow.Show));
			hotKeyManager.Run();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
