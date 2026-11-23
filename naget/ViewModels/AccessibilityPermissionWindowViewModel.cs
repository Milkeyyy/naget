using Avalonia.Controls;
using Avalonia.Threading;
using Epoxy;
using naget.Assets.Locales;
using naget.Helpers;
using System;

namespace naget.ViewModels;

[ViewModel]
public class AccessibilityPermissionWindowViewModel
{
	private readonly DispatcherTimer permissionCheckTimer;
	private bool liveCheckEnabled;
	private bool hookErrorDetected;

	public Well<Window> WindowWell { get; } = Well.Factory.Create<Window>();

	public static string WindowTitle => Resources.Accessibility_Window_Title + " - " + App.ProductName;

	public bool IsPermissionGranted { get; private set; }
	public bool IsHookError { get; private set; }
	public string StatusText { get; private set; } = string.Empty;

	public Command RequestPermissionCommand { get; }
	public Command OpenSystemSettingsCommand { get; }
	public Command RestartCommand { get; }
	public Command LaterCommand { get; }
	public Command ResetPermissionCommand { get; }

	public AccessibilityPermissionWindowViewModel()
	{
		// ウィンドウがロードされた時の処理
		WindowWell.Add(Window.LoadedEvent, () =>
		{
			App.Logger.Debug("AccessibilityPermissionWindow Loaded");
			RefreshPermissionState();
			return default;
		});

		// 権限が許可されたかどうかを定期的に確認する (ライブ確認が有効な間のみ)
		permissionCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
		permissionCheckTimer.Tick += (_, _) =>
		{
			if (!liveCheckEnabled) return;
			if (App.AccessibilityPermissionWindow?.IsVisible != true) return;
			RefreshPermissionState();
		};
		if (OperatingSystem.IsMacOS()) permissionCheckTimer.Start();

		// アクセシビリティ API へのアクセス許可を要求するコマンド
		RequestPermissionCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Request Accessibility API access");
			HotKeyHelper.RequestAccessibilityApiAccess();

			// 直後にタップ生成の確認をするとプロンプトと競合するため、更新はタイマーに任せる
			liveCheckEnabled = true;

			return default;
		});

		// システム設定のアクセシビリティ画面を開くコマンド
		OpenSystemSettingsCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Open Accessibility System Settings");
			HotKeyHelper.OpenAccessibilitySystemSettings();
			return default;
		});

		// アプリケーション再起動コマンド
		RestartCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Execute Restart App Command");
			App.RestartApplication();
			return default;
		});

		// 後で(ウィンドウを隠す)コマンド
		LaterCommand = Command.Factory.Create(() =>
		{
			App.AccessibilityPermissionWindow?.Hide();
			return default;
		});

		// 一覧に登録されない場合に TCC の登録をリセットして再起動するコマンド
		ResetPermissionCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Execute ResetPermissionCommand");
			HotKeyHelper.ResetAccessibilityPermission();
			App.RestartApplication();
			return default;
		});
	}

	public void EnableLiveCheck()
	{
		liveCheckEnabled = true;
		RefreshPermissionState();
	}

	public void SetHookError()
	{
		hookErrorDetected = true;
		liveCheckEnabled = true;
	}

	public void RefreshPermissionState()
	{
		bool isEnabled = HotKeyHelper.IsAccessibilityApiEnabled();
		if (!isEnabled && liveCheckEnabled) isEnabled = HotKeyHelper.IsAccessibilityApiEnabledLive();

		bool changed = IsPermissionGranted != isEnabled;

		// 実行中に許可されても libuiohook は以前の判定をキャッシュしているため、
		// ここではホットキーを開始せず再起動を促す
		IsPermissionGranted = isEnabled;
		IsHookError = hookErrorDetected && isEnabled;
		StatusText = IsPermissionGranted ? Resources.Accessibility_Status_Granted : Resources.Accessibility_Status_NotGranted;

		if (changed) HotKeyHelper.NotifyAccessibilityApiStateChanged();
	}
}
