using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Views.Dialog;
using System;
using System.Text;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using System.Linq;

namespace naget.Helpers;

public class Updater
{
	private TaskDialog downloadDialog;
	private int downloadProgressValue;
	private TaskDialogProgressState downloadProgressState;
	private bool downloadStarted;

	public Updater()
	{
		// ダイアログの初期化
		downloadDialog = new()
		{
			ShowProgressBar = true,
			XamlRoot = App.WindowService.GetSettingsWindow()
		};
	}

	public async void Start()
	{
		// バックグラウンドでアップデートを確認
		await CheckAndUpdateAttempt(silent: true);
	}

	public async Task ManualCheck(bool showDialog = false)
	{
		await CheckAndUpdateAttempt(silent: !showDialog);
	}

	private async Task CheckAndUpdateAttempt(bool silent)
	{
		try
		{
			var url = GetUpdateUrl();
			App.Logger.Debug($"Update URL: {url}");

			var source = new SimpleWebSource(url);
			var mgr = new UpdateManager(source);

			var newVersion = await mgr.CheckForUpdatesAsync();

			// リリースチャンネルが nightly の場合はメタデータを比較して、メタデータが異なる場合はアップデートを実行する
			if (newVersion == null && App.ProductReleaseChannel == "nightly")
			{
				try
				{
					// フィードを取得
					var feed = await ((IUpdateSource)source).GetReleaseFeed(new VelopackLoggerAdapter(), App.ProductReleaseChannel, "naget", null, null);
					if (feed != null && feed.Assets.Length > 0)
					{
						// 最新バージョンを取得
						var latest = feed.Assets.OrderByDescending(x => x.Version).First();
						var current = mgr.CurrentVersion;

						// バージョンが一致しているが、メタデータが異なる場合はアップデートとみなす
						if (current != null && latest.Version == current && latest.Version.Metadata != current.Metadata)
						{
							App.Logger.Debug($"Nightly update available (Metadata mismatch): {current.Metadata} -> {latest.Version.Metadata}");
							newVersion = new UpdateInfo(latest, false);
						}
					}
				}
				catch (Exception ex)
				{
					App.Logger.Error($"Failed to check nightly metadata: {ex.Message}");
				}
			}

			if (newVersion == null)
			{
				App.Logger.Debug("No updates available.");
				if (!silent)
				{
					await ShowNoUpdateDialog();
				}
				return;
			}

			App.Logger.Debug($"Update available: {newVersion.TargetFullRelease.Version}");

			// Show dialog to user
			if (await ShowUpdateAvailableDialog(newVersion))
			{
				await DownloadAndInstall(mgr, newVersion);
			}
		}
		catch (Exception ex)
		{
			App.Logger.Error($"Update check failed: {ex.Message}");
			if (!silent)
			{
				// エラー表示が必要ならここに追加
			}
		}
	}

	private async Task DownloadAndInstall(UpdateManager mgr, UpdateInfo info)
	{
		try
		{
			InitDownloadDialog();
			var window = (Window)downloadDialog.XamlRoot;
			if (window != null)
			{
				window.Show();
				var dialogTask = downloadDialog.ShowAsync();

				await mgr.DownloadUpdatesAsync(info, (progress) =>
				{
					// Update progress
					Dispatcher.UIThread.Post(() =>
					{
						downloadProgressValue = progress;
						downloadDialog.SetProgressBarState(downloadProgressValue, TaskDialogProgressState.Normal);
						downloadDialog.Content = string.Format(null, CompositeFormat.Parse(Resources.Updater_Dialog_Download_Downloading_Description), downloadProgressValue);
					});
				});

				// インストール中
				Dispatcher.UIThread.Post(() =>
				{
					downloadDialog.Title = Resources.Updater_Dialog_Download_Install_Title + " - " + App.ProductName;
					downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Install_Title;
					downloadDialog.Content = Resources.Updater_Dialog_Download_Install_Description;
					if (downloadDialog.Buttons.Count > 0) downloadDialog.Buttons[0].IsEnabled = false; // Disable cancel
				});

				// 少し待機
				await Task.Delay(2000);

				mgr.ApplyUpdatesAndRestart(info);
			}
		}
		catch (Exception ex)
		{
			App.Logger.Error($"Update installation failed: {ex.Message}");
			Dispatcher.UIThread.Post(() =>
			{
				downloadProgressState = TaskDialogProgressState.Error;
				downloadDialog.SetProgressBarState(downloadProgressValue, downloadProgressState);
				downloadDialog.Content = Resources.Updater_Dialog_Download_Failed_Description;
				downloadDialog.Buttons.Clear();
				downloadDialog.Buttons.Add(new TaskDialogButton(Resources.Dialog_Button_Close, TaskDialogStandardResult.Close));
			});
		}
	}

	private void InitDownloadDialog()
	{
		downloadDialog.Title = Resources.Updater_Dialog_Download_Downloading_Title + " - " + App.ProductName;
		downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Downloading_Title;
		downloadDialog.Content = string.Empty;
		downloadDialog.Buttons.Clear();
		downloadDialog.Buttons.Add(new TaskDialogButton(Resources.Dialog_Button_Cancel, TaskDialogStandardResult.Cancel));
		downloadDialog.XamlRoot = App.WindowService.GetSettingsWindow();
		downloadProgressValue = 0;
		downloadProgressState = TaskDialogProgressState.Normal;
		downloadDialog.SetProgressBarState(0, TaskDialogProgressState.Normal);
	}

	private async Task<bool> ShowUpdateAvailableDialog(UpdateInfo info)
	{
		CompositeFormat desc = CompositeFormat.Parse(Resources.Updater_Dialog_UpdateAvailable_VersionInfo);
		TaskDialog dialog = new()
		{
			Title = Resources.Updater_Dialog_UpdateAvailable_Title + " - " + App.ProductName,
			Header = Resources.Updater_Dialog_UpdateAvailable_Title,
			SubHeader = Resources.Updater_Dialog_UpdateAvailable_Description,
			Content = string.Format(null, desc, App.ProductFullVersion, info.TargetFullRelease.Version), // Check version property
			Buttons = {
				new TaskDialogButton(Resources.Dialog_Button_Yes, TaskDialogStandardResult.Yes),
				new TaskDialogButton(Resources.Dialog_Button_No, TaskDialogStandardResult.No)
			},
			XamlRoot = App.WindowService.GetSettingsWindow()
		};

		var window = (Window)dialog.XamlRoot;
		if (window != null) window.Show();
		TaskDialogStandardResult dialogResult = (TaskDialogStandardResult)await dialog.ShowAsync();

		return dialogResult == TaskDialogStandardResult.Yes;
	}

	private async Task ShowNoUpdateDialog()
	{
		CompositeFormat desc = CompositeFormat.Parse(Resources.Updater_Dialog_UpdateNotAvailable_Description);
		await SuperDialog.Info(
			App.WindowService.GetSettingsWindow()!,
			Resources.Updater_Dialog_UpdateNotAvailable_Title,
			string.Format(null, desc, App.ProductFullVersion)
		);
	}

	private static string GetUpdateUrl()
	{
		// 1. コマンドライン引数から URL を探す
		for (int i = 0; i < App.CmdArgs.Length; i++)
		{
			if ((App.CmdArgs[i].Equals("/UpdateUrl", StringComparison.OrdinalIgnoreCase) ||
				 App.CmdArgs[i].Equals("--update-url", StringComparison.OrdinalIgnoreCase)) &&
				i + 1 < App.CmdArgs.Length)
			{
				string url = App.CmdArgs[i + 1];
				App.Logger.Debug($"Using custom Update URL from args: {url}");
				return url;
			}
		}

		// 2. 指定されていない場合はデフォルトの URL を返す
		// Velopack はディレクトリの URL を期待する
		return $"https://nagetupd.milkeyyy.com/";
	}
}
internal class VelopackLoggerAdapter : Velopack.Logging.IVelopackLogger
{
	public void Log(Velopack.Logging.VelopackLogLevel level, string? message, Exception? exception = null)
	{
		App.Logger.Debug($"[Velopack] {level}: {message}");
	}
}
