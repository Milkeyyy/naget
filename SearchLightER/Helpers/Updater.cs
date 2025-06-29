using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Common;
using naget.Views;
using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace naget.Helpers;

public class Updater : SparkleUpdater
{
	bool downloadStarted;
	TaskDialog downloadDialog;
	private int downloadProgressValue;
	TaskDialogProgressState downloadProgressState;

	public Updater() : base(
		"https://update-naget.milkeyyy.com/appcast_" + App.ProductReleaseChannel +  "_" + RuntimeInformation.RuntimeIdentifier + ".xml",
		new Ed25519Checker(
			SecurityMode.OnlyVerifySoftwareDownloads,
			"xtbwCBV7esFcqM9thhlze+82NosbQqsT1inUwWurRZE="
		)
	)
	{
		AppCastHelper.AppCastFilter = new CustomAppCastFilter();
		RelaunchAfterUpdate = true;
		UseNotificationToast = false;
		CustomInstallerArguments = "/SILENT";
		// GitHub の Releases からダウンロードする際はこれを無効にする
		// 参考: https://github.com/NetSparkleUpdater/NetSparkle/issues/546#issuecomment-1869321315
		CheckServerFileName = false;

		// ダウンロード処理関連イベント
		DownloadStarted += Updater_DownloadStarted;
		DownloadCanceled += Updater_DownloadCanceled;
		DownloadFinished += Updater_DownloadFinished;
		DownloadMadeProgress += Updater_DownloadMadeProgress;
		DownloadHadError += Updater_DownloadHadError;
		// アプリケーション終了イベント
		CloseApplication += () => App.Exit();

		// アプリ終了前のイベント
		PreparingToExit += (s, e) =>
		{
			// コンフィグ等の保存を実行
			//App.Save();
		};

		// ダウンロード処理用ダイアログの初期化
		downloadDialog = new()
		{
			ShowProgressBar = true,
			XamlRoot = App.SettingsWindow
		};

		// ダウンロード処理ダイアログが閉じられる時のイベント
		downloadDialog.Closing += (s, e) =>
		{
			// キャンセルボタンが押された場合はダウンロード処理をキャンセルする
			if ((TaskDialogStandardResult)e.Result == TaskDialogStandardResult.Cancel)
			{
				CancelFileDownload();
			}
			else
			{
				e.Cancel = true;
			}
		};

		// ダイアログの初期化
		InitDownloadDialog();
	}

	private void InitDownloadDialog()
	{
		// ダイアログの初期化
		downloadDialog.Title = Resources.Updater_Dialog_Download_Downloading_Title + " - " + App.ProductName;
		downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Downloading_Title;
		downloadDialog.Content = string.Empty;
		downloadDialog.Buttons = [
			new TaskDialogButton(Resources.Dialog_Button_Cancel, TaskDialogStandardResult.Cancel)
		];
	}

	private async void Updater_DownloadStarted(AppCastItem item, string path)
	{
		// ダウンロード処理実行済みフラグ
		downloadStarted = true;
		// ダイアログの初期化
		InitDownloadDialog();
		// プログレスバーの初期値を設定
		downloadProgressValue = 0;
		downloadProgressState = TaskDialogProgressState.Indeterminate;
		downloadDialog.SetProgressBarState(downloadProgressValue, downloadProgressState);
		// ダイアログを表示する
		((Window)downloadDialog.XamlRoot).Show();
		await downloadDialog.ShowAsync();
	}

	private void Updater_DownloadCanceled(AppCastItem item, string path)
	{
		
	}

	private async void Updater_DownloadFinished(AppCastItem item, string path)
	{
		// キャンセルボタンを無効化する
		downloadDialog.Buttons[0].IsEnabled = false;
		// プログレスバーの値を設定
		downloadProgressValue = 100;
		downloadProgressState = TaskDialogProgressState.Indeterminate;
		downloadDialog.SetProgressBarState(downloadProgressValue, downloadProgressState);
		// タイトル等を更新
		downloadDialog.Title = Resources.Updater_Dialog_Download_Install_Title + " - " + App.ProductName;
		downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Install_Title;
		downloadDialog.Content = Resources.Updater_Dialog_Download_Install_Description;

		// ダウンロードが実行されていない場合は新たにダイアログを表示する
		if (!downloadStarted)
		{
			((Window)downloadDialog.XamlRoot).Show();
			var t = downloadDialog.ShowAsync();
		}

		// 5秒間待機
		await Task.Delay(5000);

		// アップデートのインストールを実行する
		await InstallUpdate(item, path);
	}

	private static readonly CompositeFormat CachedDownloadDescriptionFormat = CompositeFormat.Parse(Resources.Updater_Dialog_Download_Downloading_Description);
	private void Updater_DownloadMadeProgress(object sender, AppCastItem item, NetSparkleUpdater.Events.ItemDownloadProgressEventArgs args)
	{
		// 進行状況の表示を更新する
		downloadDialog.Content = string.Format(null, CachedDownloadDescriptionFormat, downloadProgressValue);
		// プログレスバーの値を更新する
		downloadProgressValue = args.ProgressPercentage;
		downloadProgressState = TaskDialogProgressState.Normal;
		downloadDialog.SetProgressBarState(downloadProgressValue, downloadProgressState);
	}

	private void Updater_DownloadHadError(AppCastItem item, string? path, Exception exception)
	{
		// プログレスバーの値を設定
		downloadProgressState = TaskDialogProgressState.Error;
		downloadDialog.SetProgressBarState(downloadProgressValue, downloadProgressState);
		downloadDialog.Title = Resources.Updater_Dialog_Download_Failed_Title + " - " + App.ProductName;
		downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Failed_Title;
		downloadDialog.Content = Resources.Updater_Dialog_Download_Failed_Description;
		downloadDialog.Buttons = [
			new TaskDialogButton(Resources.Dialog_Button_Close, TaskDialogStandardResult.Close)
		];
	}

	public void Start()
	{
		var t = StartLoop(true);
	}

	public async Task ManualCheck(bool showDialog = false)
	{
		Debug.WriteLine("Start manual update check");
		UpdateInfo info = await CheckForUpdatesQuietly();
		
		Debug.WriteLine($"- NetSparkle Status: {info.Status} / {info.Updates.Count}");

		//string updVersion;
		//int updInternalVersion;
		//string updReleaseChannel;
		//string updReleaseNumber;
		//foreach (var u in info.Updates)
		//{
		//	if (u != null)
		//	{
		//		try
		//		{
		//			Debug.WriteLine($"       Title: {u.Title}");
		//			Debug.WriteLine($" Description: {u.Description}");
		//			Debug.WriteLine($"     Version: {u.Version}");
		//			Debug.WriteLine($"ShortVersion: {u.ShortVersion}");
		//			Debug.WriteLine($"     Channel: {u.Channel}");
		//			Debug.WriteLine($"          OS: {u.OperatingSystem}");
		//			Debug.WriteLine($"        Date: {u.PublicationDate}");

		//			updVersion = u.ShortVersion ?? string.Empty;
		//			updInternalVersion = Utils.ConvertToInt(updVersion.Replace(".", string.Empty), 0);
		//			var vs = u.Version?.Split("-");
		//			if (vs == null)
		//			{
		//				Debug.WriteLine($"Version comparison failed: Splitted Version Strings is Null");
		//				break;
		//			}
		//			updReleaseChannel = vs[1].Split(".")[0] ?? string.Empty;
		//			updReleaseNumber = vs[1].Split(".")[1] ?? string.Empty;

		//			Debug.WriteLine($"NetSparkle UpdateStatus: {info.Status}");

		//			// バージョン比較を行う
		//			if (App.ProductInternalVersion < updInternalVersion)
		//			{
		//				Debug.WriteLine($" - naget UpdateStatus: Available - Internal Version");
		//				info.Status = UpdateStatus.UpdateAvailable;
		//				break;
		//			}
		//			// リリース番号が整数である (コミットハッシュとかではない) 場合はそれを比較する
		//			else if (Utils.ConvertToInt(App.ProductReleaseNumber, -1) != -1 && Utils.ConvertToInt(updReleaseNumber, -1) != -1)
		//			{
		//				Debug.WriteLine("Compare Release Number");
		//				if (Utils.ConvertToInt(App.ProductReleaseNumber) < Utils.ConvertToInt(updReleaseNumber))
		//				{
		//					Debug.WriteLine($" - naget UpdateStatus: Available - Release Number");
		//					info.Status = UpdateStatus.UpdateAvailable;
		//					break;
		//				}
		//			}
		//			// それ以外の場合はリリース番号が異なるかどうかを比較する
		//			else
		//			{
		//				if (App.ProductReleaseNumber != updReleaseNumber)
		//				{
		//					Debug.WriteLine($" - naget UpdateStatus: Available - Release Number");
		//					info.Status = UpdateStatus.UpdateNotAvailable;
		//					break;
		//				}
		//				Debug.WriteLine($" - naget Status: Not Available");
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Debug.WriteLine($"Version comparison failed: {ex.Message}");
		//			Debug.WriteLine($"{ex.StackTrace}");
		//			info.Status = UpdateStatus.CouldNotDetermine;
		//			return;
		//		}
		//	}
		//}

		if (info.Status == UpdateStatus.UpdateAvailable)
		{
			// アップデート確認ダイアログを表示する
			await ShowDialogAsync(info.Updates[0]);
			
		}
		// 既に最新バージョンの場合
		else if(info.Status == UpdateStatus.UpdateNotAvailable && showDialog)
		{
			Debug.WriteLine("Show Update not available dialog");
			CompositeFormat desc = CompositeFormat.Parse(Resources.Updater_Dialog_UpdateNotAvailable_Description);
			await SuperDialog.Info(
				App.SettingsWindow,
				Resources.Updater_Dialog_UpdateNotAvailable_Title,
				string.Format(null, desc, App.ProductFullVersion)
			);
			return;
		}
	}

	public async Task ShowDialogAsync(AppCastItem info)
	{
		CompositeFormat desc = CompositeFormat.Parse(Resources.Updater_Dialog_UpdateAvailable_VersionInfo);
		TaskDialog dialog = new()
		{
			Title = Resources.Updater_Dialog_UpdateAvailable_Title + " - " + App.ProductName,
			Header = Resources.Updater_Dialog_UpdateAvailable_Title,
			SubHeader = Resources.Updater_Dialog_UpdateAvailable_Description,
			Content = string.Format(null, desc, App.ProductFullVersion, info.Version),
			Buttons = {
				new TaskDialogButton(Resources.Dialog_Button_Yes, TaskDialogStandardResult.Yes),
				new TaskDialogButton(Resources.Dialog_Button_No, TaskDialogStandardResult.No)
			},
			XamlRoot = App.SettingsWindow
		};

		Debug.WriteLine("Show Dialog");
		((Window)dialog.XamlRoot).Show();
		TaskDialogStandardResult dialogResult = (TaskDialogStandardResult)await dialog.ShowAsync();

		if (dialogResult == TaskDialogStandardResult.Yes)
		{
			// アップデート処理実行
			await InitAndBeginDownload(info);
		}
	}
}

public class CustomAppCastFilter : IAppCastFilter
{
	public IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)
	{
		string updVersion;
		int updInternalVersion;
		string updReleaseChannel;
		string updReleaseNumber;
		return items.Where((item) =>
		{
			try
			{
				Debug.WriteLine("Current Info");
				Debug.WriteLine($"        Title: {App.ProductName}");
				Debug.WriteLine($"      Version: {App.ProductFullVersion}");
				Debug.WriteLine($" ShortVersion: {App.ProductVersion}");
				Debug.WriteLine($"	 Release Ch: {App.ProductReleaseChannel}");
				Debug.WriteLine($"  Release Num: {App.ProductReleaseNumber}");
				Debug.WriteLine($"==============================");
				Debug.WriteLine("Latest Info");
				Debug.WriteLine($"        Title: {item.Title}");
				Debug.WriteLine($"  Description: {item.Description}");
				Debug.WriteLine($"      Version: {item.Version}");
				Debug.WriteLine($" ShortVersion: {item.ShortVersion}");
				Debug.WriteLine($"      Channel: {item.Channel}");
				Debug.WriteLine($"           OS: {item.OperatingSystem}");
				Debug.WriteLine($"         Date: {item.PublicationDate}");

				updVersion = item.ShortVersion ?? string.Empty;
				updInternalVersion = Utils.ConvertToInt(updVersion.Replace(".", string.Empty), 0);
				var vs = item.Version?.Split("-");
				if (vs == null)
				{
					Debug.WriteLine("Version comparison failed: Splitted Version Strings is Null");
					return false;
				}
				else if (vs.Length == 0)
				{
					Debug.WriteLine("Version comparison failed: Splitted Version Strings Count is 0");
					return false;
				}

				string sp = ".";
				if (vs[1].Contains('+')) sp = "+";

				updReleaseChannel = vs[1].Split(sp)[0] ?? string.Empty;
				updReleaseNumber = vs[1].Split(sp)[1] ?? string.Empty;

				Debug.WriteLine($"   Release Ch: {updReleaseChannel}");
				Debug.WriteLine($"  Release Num: {updReleaseNumber}");

				// バージョン比較を行う
				if (App.ProductInternalVersion < updInternalVersion)
				{
					Debug.WriteLine(" - naget UpdateStatus: Available - Internal Version");
					return true;
				}
				// リリース番号が整数である (コミットハッシュとかではない) 場合はそれを比較する
				else if (Utils.ConvertToInt(App.ProductReleaseNumber, -1) != -1 && Utils.ConvertToInt(updReleaseNumber, -1) != -1)
				{
					Debug.WriteLine("Compare Release Number");
					if (Utils.ConvertToInt(App.ProductReleaseNumber) < Utils.ConvertToInt(updReleaseNumber))
					{
						Debug.WriteLine(" - naget UpdateStatus: Available - Release Number");
						return true;
					}
				}
				// それ以外の場合はリリース番号が異なるかどうかを比較する
				else
				{
					if (App.ProductReleaseNumber != updReleaseNumber)
					{
						Debug.WriteLine(" - naget UpdateStatus: Available - Release Number");
						return true;
					}
					Debug.WriteLine(" - naget Status: Not Available");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Version comparison failed: {ex.Message}");
				Debug.WriteLine($"{ex.StackTrace}");
				return false;
			}
			return false;
		});
	}
}
