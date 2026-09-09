using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Epoxy;
using naget.Assets.Locales;
using naget.Models.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class InAppBrowserViewModel
{
	public Well<UserControl> ViewWell { get; } = Well.Factory.Create<UserControl>();

	public string StartPageUrl { get; set; } = string.Empty;

	// 閲覧データの保存先変更は WebView2 (Windows) のみ対応
	public bool IsDataFolderSettingVisible { get; } = OperatingSystem.IsWindows();

	public string DataFolderPath { get; set; } = string.Empty;

	public bool DataFolderNoteTextIsVisible { get; set; }

	public Command BrowseDataFolderCommand { get; }

	public Command ResetDataFolderCommand { get; }

	public Command RestartAppCommand { get; }

	private bool ViewIsLoaded;

	// ビューのロード時にコンフィグから読み込んだ値 (再起動が必要かの判定に使用)
	private string loadedDataFolderPath = string.Empty;

	public InAppBrowserViewModel()
	{
		// ビューがロードされた時の処理
		ViewWell.Add(Control.LoadedEvent, () =>
		{
			App.Logger.Debug("InAppBrowserView Loaded");
			// コンフィグから設定を読み込む
			StartPageUrl = ConfigManager.Config.BrowserWindow.StartPage;
			loadedDataFolderPath = ConfigManager.Config.BrowserWindow.DataFolder ?? string.Empty;
			DataFolderPath = loadedDataFolderPath;
			DataFolderNoteTextIsVisible = false;
			ViewIsLoaded = true;
			return default;
		});

		// 閲覧データの保存先フォルダーを選択するコマンド
		BrowseDataFolderCommand = Command.Factory.Create(async () =>
		{
			App.Logger.Debug("Execute Browse Data Folder Command");
			IReadOnlyList<IStorageFolder> folders = await App.SettingsWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
			{
				Title = Resources.Settings_InAppBrowser_BrowseDataFolder,
				AllowMultiple = false
			});
			IStorageFolder? folder = folders.FirstOrDefault();
			if (folder != null)
			{
				DataFolderPath = folder.Path.LocalPath;
			}
		});

		// 閲覧データの保存先を既定に戻すコマンド
		ResetDataFolderCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Execute Reset Data Folder Command");
			DataFolderPath = string.Empty;
			return default;
		});

		// アプリケーション再起動コマンド
		RestartAppCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Execute Restart App Command");
			App.RestartApplication();
			return default;
		});
	}

	[PropertyChanged(nameof(StartPageUrl))]
	private ValueTask StartPageUrlChangedAsync(string value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		App.Logger.Debug($"Start Page Url Changed: {value}");
		// コンフィグのスタートページ設定を更新
		ConfigManager.Config.BrowserWindow.StartPage = value;
		return default;
	}

	[PropertyChanged(nameof(DataFolderPath))]
	private ValueTask DataFolderPathChanged(string value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		App.Logger.Debug($"Data Folder Path Changed: {value}");
		// コンフィグの閲覧データ保存先を更新
		ConfigManager.Config.BrowserWindow.DataFolder = value;
		// 適用には再起動が必要なため、ロード時の値から変更された場合は注意書きを表示する
		DataFolderNoteTextIsVisible = value != loadedDataFolderPath;
		return default;
	}
}
