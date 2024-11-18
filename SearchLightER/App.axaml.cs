using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GlobalHotKeys.Native.Types;
using SearchLight.Models;
using SearchLight.ViewModels;
using SearchLight.Views;
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

	public override void OnFrameworkInitializationCompleted()
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

			// ホットキー
			var hotKeyManager = new GlobalHotKeys.HotKeyManager();
			var hotKeySubscription = hotKeyManager.Register(
				VirtualKeyCode.KEY_D,
				Modifiers.Control | Modifiers.Alt // Ctrl + Alt
			);

			desktop.Exit += (sender, args) =>
			{
				hotKeySubscription.Dispose();
				hotKeyManager.Dispose();
			};

			MainWindow = new MainWindow();
			SettingsWindow = new SettingsWindow();
			BrowserWindow = new BrowserWindow();

			desktop.MainWindow = SettingsWindow;

			// ホットキー押下時イベント
			hotKeyManager.HotKeyPressed
				.ObserveOn(Avalonia.ReactiveUI.AvaloniaScheduler.Instance)
				.Subscribe(_ => MainWindow.Show()); // 検索画面を開く
				//.Subscribe(_ => SettingsWindow.Show()); // 設定画面を開く
				//.Subscribe(hotKey => MainWindowViewModel.Text += $"HotKey: Id={hotKey.Id}, Key={hotKey.Key}, Modifiers={hotKey.Modifiers}{Environment.NewLine}");
		}

		base.OnFrameworkInitializationCompleted();
	}
}
