using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using naget.Common;
using naget.Helpers;
using naget.Models.BrowserHistory;
using naget.Models.Config;
using naget.Models.SearchEngine;
using naget.ViewModels;
using naget.Views;
using naget.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace naget;

public class App : Application
{
	public static string[] CmdArgs { get; private set; } = [];

	private static string _name = string.Empty;
	public static string ProductName => _name;

	private static string _version = string.Empty;
	public static string ProductVersion => _version;

	public static int ProductInternalVersion { get { return Utils.ConvertToInt(_version.Replace(".", string.Empty), 0); } }

	private static string _releaseChannel = string.Empty;
	public static string ProductReleaseChannel => _releaseChannel;

	private static string _releaseNumber = string.Empty;
	public static string ProductReleaseNumber => _releaseNumber;

	private static string _fullVersion = string.Empty;
	public static string ProductFullVersion => _fullVersion;

	private static string _copyright = string.Empty;
	public static string ProductCopyright => _copyright;

	private static List<Dictionary<string, object>> Libraries = [];

	/// <summary>
	/// コンフィグ等のファイルを保存するフォルダー
	/// </summary>
	public static string ConfigFolder => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);

	/// <summary>
	/// アプリ内ブラウザーの閲覧データ保存先を取得する (設定が空欄の場合はコンフィグフォルダー配下の WebView フォルダー)
	/// </summary>
	public static string GetBrowserDataFolder()
	{
		string defaultFolder = Path.Join(ConfigFolder, "WebView");
		string configured = ConfigManager.Config.BrowserWindow.DataFolder?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(configured)) return defaultFolder;

		try
		{
			return Path.GetFullPath(configured);
		}
		catch (Exception ex)
		{
			// 無効なパスが設定されていた場合は既定にフォールバックする
			Logger.Warn($"Invalid browsing data folder: {configured} ({ex.Message})");
			return defaultFolder;
		}
	}

	public static Logger Logger { get; private set; }

	public static Updater Updater { get; private set; }
	public static WindowService WindowService { get; private set; } = new();

	public static Window? AboutWindow { get; private set; }
	public static Window? UpdateCompleteWindow { get; private set; }
	public static Window? MainWindow { get; private set; }
	public static Window? SettingsWindow { get; private set; }

	// BrowserWindow の遅延初期化 (Lazy Loading)
	// 実際にアクセスされるまでウィンドウと NativeWebView の作成を遅延させる。
	private static Window? _browserWindow;
	public static Window BrowserWindow
	{
		get
		{
			if (_browserWindow == null)
			{
				_browserWindow = new BrowserWindow();
			}
			return _browserWindow;
		}
	}

	public override void Initialize()
	{
		Assembly asm = Assembly.GetExecutingAssembly();

		if (asm.GetName().Name != null) _name = asm.GetName().Name ?? "naget";

		// バージョン情報を読み込む
		string info;
		using var verStream = asm.GetManifestResourceStream("naget.build.json");
		if (verStream != null)
		{
			using var reader = new StreamReader(verStream);
			info = reader.ReadToEnd();
			var infoDict = JsonSerializer.Deserialize<Dictionary<string, string>>(info);
			if (infoDict != null)
			{
				_version = infoDict.GetValueOrDefault("version", asm.GetName().Version?.ToString() ?? "0.0.0");
				_releaseChannel = infoDict.GetValueOrDefault("release_channel", "unknown");
				_releaseNumber = infoDict.GetValueOrDefault("release_number", "0");
				_fullVersion = infoDict.GetValueOrDefault("full_version", "0.0.0-unknown.0");
			}
		}

		// ライブラリー一覧を読み込む
		string libs;
		using var libsStream = asm.GetManifestResourceStream("naget.library.json");
		if (libsStream != null)
		{
			using var reader = new StreamReader(libsStream);
			libs = reader.ReadToEnd();
			Libraries = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(libs) ?? [];
		}

		// ロガー
		Logger = Logger.GetInstance();
		Logger.Info("Application Starting...");

		// フォルダーを作成する
		Logger.Debug("Config Directory: " + ConfigFolder);
		Directory.CreateDirectory(ConfigFolder);

		// コンフィグを読み込む
		ConfigManager.Load();

		// 検索エンジンのリストを読み込む
		SearchEngineManager.Load();

		BrowserHistoryManager.Load();

		// 言語設定を適用
		Assets.Locales.Resources.Culture = new CultureInfo(ConfigManager.Config.Language);
		Logger.Debug($"Language: {ConfigManager.Config.Language}");

		// 初期化
		AvaloniaXamlLoader.Load(this);
	}

	public static void Save()
	{
		// SharpHook を停止
		HotKeyHelper.Stop();
		// コンフィグを保存
		ConfigManager.Save();
		// 検索エンジンデータを保存
		SearchEngineManager.Save();
	}

	public static void Exit()
	{
		// ホットキーヘルパーの停止とコンフィグ等の保存
		Save();
		// 終了
		Environment.Exit(0);
	}

	public static void RestartApplication()
	{
		// 1. 新しいプロセスを起動するための情報を設定する
		var processPath = Environment.ProcessPath;

		if (processPath != null)
		{
			App.Logger.Debug("アプリケーションを再起動します...");

			// 2. 新しいプロセスを開始する
			Process.Start(new ProcessStartInfo(processPath)
			{
				UseShellExecute = true // OSのシェル経由で起動する
			});
		}
		else
		{
			App.Logger.Debug("再起動に失敗しました: 実行ファイルのパスを取得できませんでした。");
		}

		// 3. 現在のプロセスを終了する
		Exit();
	}

	/// <summary>
	/// ライブラリーの情報を取得する
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static Dictionary<string, object>? GetLibraryInfo(string id)
	{
		return Libraries.FirstOrDefault(d => d.ContainsKey("PackageId") && d["PackageId"].ToString() == id);
	}

	/// <summary>
	/// アプリ全体のテーマを変更する
	/// </summary>
	/// <param name="name"></param>
	public static void ChangeTheme(string name)
	{
		if (Current == null)
		{
			App.Logger.Debug("Current Application is null, cannot change theme.");
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
				App.Logger.Debug($"Unknown theme selected: {name}");
				break;
		}
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			CmdArgs = desktop.Args ?? [];

			DataContext = new AppViewModel(); // 通知領域メニューのためのビューモデル

			// 各ウィンドウ
			AboutWindow = new AboutWindow();
			UpdateCompleteWindow = new UpdateCompleteWindow();
			MainWindow = new MainWindow();
			SettingsWindow = new SettingsWindow();
			// BrowserWindow は遅延初期化するためここでは作成しない
			// BrowserWindow = new BrowserWindow();

			// テーマを適用
			ChangeTheme(ConfigManager.Config.Theme);

			// ホットキーの登録
			HotKeyHelper.Run();

			// ループの開始
			Updater = new();
			Updater.Start();

			// アップデート完了引数が渡された場合はアップデート完了ダイアログを表示する
			if (CmdArgs.Contains("/UpdateComplete"))
			{
				SettingsWindow.Show();
				UpdateCompleteWindow.ShowDialog(SettingsWindow);
			}
		}

		base.OnFrameworkInitializationCompleted();
	}
}
