using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SearchLight.Models.Config;
using SearchLight.Models.Config.HotKey;
using SearchLight.Models.SearchEngine;
using SearchLight.ViewModels;
using SearchLight.Views;
using SharpHook.Native;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SearchLight;

public class App : Application
{
	private static string _name = string.Empty;
	public static string ProductName => _name;

	private static string _version = string.Empty;
	public static string ProductVersion => _version;

	/// <summary>
	/// コンフィグ等のファイルを保存するフォルダー
	/// </summary>
	public static string ConfigFolder => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);

	public static Window? MainWindow { get; private set; }
	public static Window? SettingsWindow { get; private set; }
	public static Window? BrowserWindow { get; private set; }
	public static Models.Config.HotKey.HotKeyManager? HotKeyManager { get; private set; }

	public override void Initialize()
	{
		var a = Assembly.GetExecutingAssembly().GetName();
		if (a.Name != null) _name = a.Name;
		if (a.Version != null) _version = a.Version.ToString();
		AvaloniaXamlLoader.Load(this);
	}

	public static void Exit()
	{
		HotKeyManager?.Dispose();
		// コンフィグを保存
		ConfigManager.Save();
		// 検索エンジンデータを保存
		SearchEngineManager.Save();
		// 終了
		Environment.Exit(0);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			DataContext = new AppViewModel(); // 通知領域メニューのためのビューモデル
			desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			// フォルダーを作成する
			Debug.WriteLine("Config Directory: " + ConfigFolder);
			Directory.CreateDirectory(ConfigFolder);

			// コンフィグを読み込む
			ConfigManager.Load();

			// 検索エンジンのリストを読み込む
			SearchEngineManager.Load();

			// 言語設定を適用
			Assets.Locales.Resources.Culture = new CultureInfo(ConfigManager.Config.Language);
			Debug.WriteLine($"Language: {ConfigManager.Config.Language}");

			MainWindow = new MainWindow();
			SettingsWindow = new SettingsWindow();
			BrowserWindow = new BrowserWindow();

			desktop.MainWindow = SettingsWindow;

			// ホットキーの登録
			HotKeyManager = new Models.Config.HotKey.HotKeyManager();
			HotKeyManager.Register(new HotKeyGroup([KeyCode.VcLeftControl, KeyCode.VcLeftAlt, KeyCode.VcA]));
			HotKeyManager.Register(new HotKeyGroup([KeyCode.VcLeftControl, KeyCode.VcLeftAlt, KeyCode.VcQ]));
			HotKeyManager.Run();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
