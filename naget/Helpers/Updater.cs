using Avalonia.Controls;
using Avalonia.Threading;
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
	private ProgressDialogWindow downloadDialog;
	private int downloadProgressValue;
	private ProgressState downloadProgressState;
	private bool downloadStarted;

	public Updater()
	{
		// ダイアログの初期化
		downloadDialog = new()
		{
			ShowProgressBar = true,
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
			var owner = App.WindowService.GetSettingsWindow();
			if (owner != null)
			{
				owner.Show();
				var dialogTask = downloadDialog.ShowAsync(owner);

				await mgr.DownloadUpdatesAsync(info, (progress) =>
				{
					// Update progress
					Dispatcher.UIThread.Post(() =>
					{
						downloadProgressValue = progress;
						downloadDialog.SetProgressBarState(downloadProgressValue, ProgressState.Normal);
						downloadDialog.DialogContent = string.Format(null, CompositeFormat.Parse(Resources.Updater_Dialog_Download_Downloading_Description), downloadProgressValue);
					});
				});

				// インストール中
				Dispatcher.UIThread.Post(() =>
				{
					downloadDialog.DialogTitle = Resources.Updater_Dialog_Download_Install_Title + " - " + App.ProductName;
					downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Install_Title;
					downloadDialog.DialogContent = Resources.Updater_Dialog_Download_Install_Description;
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
				downloadProgressState = ProgressState.Error;
				downloadDialog.SetProgressBarState(downloadProgressValue, downloadProgressState);
				downloadDialog.DialogContent = Resources.Updater_Dialog_Download_Failed_Description;
				downloadDialog.Buttons.Clear();
				downloadDialog.Buttons.Add(new ProgressDialogButton(Resources.Dialog_Button_Close, ProgressDialogResult.Close));
			});
		}
	}

	private void InitDownloadDialog()
	{
		downloadDialog = new ProgressDialogWindow();
		downloadDialog.DialogTitle = Resources.Updater_Dialog_Download_Downloading_Title + " - " + App.ProductName;
		downloadDialog.SubHeader = Resources.Updater_Dialog_Download_Downloading_Title;
		downloadDialog.DialogContent = string.Empty;
		downloadDialog.ShowProgressBar = true;
		downloadDialog.Buttons.Clear();
		downloadDialog.Buttons.Add(new ProgressDialogButton(Resources.Dialog_Button_Cancel, ProgressDialogResult.Cancel));
		downloadProgressValue = 0;
		downloadProgressState = ProgressState.Normal;
		downloadDialog.SetProgressBarState(0, ProgressState.Normal);
	}

	private async Task<bool> ShowUpdateAvailableDialog(UpdateInfo info)
	{
		CompositeFormat desc = CompositeFormat.Parse(Resources.Updater_Dialog_UpdateAvailable_VersionInfo);
		var dialog = new ProgressDialogWindow
		{
			DialogTitle = Resources.Updater_Dialog_UpdateAvailable_Title + " - " + App.ProductName,
			SubHeader = Resources.Updater_Dialog_UpdateAvailable_Description,
			DialogContent = string.Format(null, desc, App.ProductFullVersion, info.TargetFullRelease.Version.ToFullString()),
			ShowProgressBar = false,
		};
		dialog.Buttons.Add(new ProgressDialogButton(Resources.Dialog_Button_Yes, ProgressDialogResult.Yes));
		dialog.Buttons.Add(new ProgressDialogButton(Resources.Dialog_Button_No, ProgressDialogResult.No));

		var owner = App.WindowService.GetSettingsWindow();
		if (owner != null)
		{
			owner.Show();
			var dialogResult = await dialog.ShowAsync(owner);
			return dialogResult == ProgressDialogResult.Yes;
		}
		return false;
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
