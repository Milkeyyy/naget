using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using naget.Models.Config;
using naget.Models.SearchEngine;
using naget.ViewModels;
using naget.Views;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace naget;

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

	private static SparkleUpdater _sparkle;

	public static Window? MainWindow { get; private set; }
	public static Window? SettingsWindow { get; private set; }
	public static Window? BrowserWindow { get; private set; }

	public override void Initialize()
	{
		var a = Assembly.GetExecutingAssembly().GetName();
		if (a.Name != null) _name = a.Name;
		if (a.Version != null) _version = a.Version.ToString();
		AvaloniaXamlLoader.Load(this);
	}

	public static void Exit()
	{
		ConfigManager.HotKeyManager.Dispose();
		// コンフィグを保存
		ConfigManager.Save();
		// 検索エンジンデータを保存
		SearchEngineManager.Save();
		// 終了
		Environment.Exit(0);
	}

	public static void RestartApplication()
	{
		// 1. 新しいプロセスを起動するための情報を設定する
		var processPath = Environment.ProcessPath;
		
		if (processPath != null)
		{
			Debug.WriteLine("アプリケーションを再起動します...");

			// 2. 新しいプロセスを開始する
			Process.Start(new ProcessStartInfo(processPath)
			{
				UseShellExecute = true // OSのシェル経由で起動する
			});
		}
		else
		{
			Debug.WriteLine("再起動に失敗しました: 実行ファイルのパスを取得できませんでした。");
		}

		// 3. 現在のプロセスを終了する
		Exit();
	}

	/// <summary>
	/// アプリ全体のテーマを変更する
	/// </summary>
	/// <param name="name"></param>
	public static void ChangeTheme(string name)
	{
		if (Current == null)
		{
			Debug.WriteLine("Current Application is null, cannot change theme.");
			return;
		}

		switch (name)
		{
			case "Default":
				Current.RequestedThemeVariant = ThemeVariant.Default;
				break;
			case "Light":
				Current.RequestedThemeVariant = ThemeVariant.Light;
				break;
			case "Dark":
				Current.RequestedThemeVariant = ThemeVariant.Dark;
				break;
			default:
				// 未知のテーマの場合はデフォルトに戻す
				Current.RequestedThemeVariant = ThemeVariant.Default;
				Debug.WriteLine($"Unknown theme selected: {name}");
				break;
		}
	}

	private static async void StartSparkle()
	{
		await _sparkle.StartLoop(true);
		// 手動アップデートチェック
		//await ManualUpdateCheck();
	}

	public static async Task ManualUpdateCheck()
	{
		await _sparkle.CheckForUpdatesAtUserRequest();
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

			// テーマを適用
			ChangeTheme(ConfigManager.Config.Theme);

			MainWindow = new MainWindow();
			SettingsWindow = new SettingsWindow();
			BrowserWindow = new BrowserWindow();

			desktop.MainWindow = SettingsWindow;

			// ホットキーの登録
			ConfigManager.HotKeyManager.Run();

			// Sparkle の初期化
			_sparkle = new(
				"https://naget.milkeyyy.com/appcast.xml",
				new Ed25519Checker(
					SecurityMode.OnlyVerifySoftwareDownloads,
					"xtbwCBV7esFcqM9thhlze+82NosbQqsT1inUwWurRZE="
				)
			)
			{
				UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(new WindowIcon(AssetLoader.Open(new Uri("avares://naget/Assets/Icon.ico")))),
				RelaunchAfterUpdate = false,
				UseNotificationToast = true,
				CustomInstallerArguments = "/SILENT"
			};
			
			_sparkle.PreparingToExit += (sender, e) =>
			{
				Exit();
				// アプリケーション終了時に Sparkle のループを停止
				//e.Cancel = true;
				//_sparkle.StopLoop();
				//Debug.WriteLine("Sparkle loop stopped.");
			};

			// ループの開始
			StartSparkle();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
