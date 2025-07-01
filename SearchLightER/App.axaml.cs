using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using naget.Common;
using naget.Helpers;
using naget.Models.Config;
using naget.Models.SearchEngine;
using naget.ViewModels;
using naget.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

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

	public static string ProductFullVersion
	{
		get
		{
			string st = ".";
			// リリース番号が数字でない場合は . ではなく + で区切る
			var rn = Utils.ConvertToInt(ProductReleaseNumber, -1);
			if (rn == -1) st = "+";
			return $"{ProductVersion}-{ProductReleaseChannel}{st}{ProductReleaseNumber}";
		}
	}

	private static string _copyright = string.Empty;
	public static string ProductCopyright => _copyright;

	private static List<Dictionary<string, object>> Libraries = [];

	/// <summary>
	/// コンフィグ等のファイルを保存するフォルダー
	/// </summary>
	public static string ConfigFolder => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);

	public static Updater Updater { get; private set; }

	public static Window? AboutWindow { get; private set; }
	public static Window? UpdateCompleteWindow { get; private set; }
	public static Window? MainWindow { get; private set; }
	public static Window? SettingsWindow { get; private set; }
	public static Window? BrowserWindow { get; private set; }

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
			BrowserWindow = new BrowserWindow();

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
