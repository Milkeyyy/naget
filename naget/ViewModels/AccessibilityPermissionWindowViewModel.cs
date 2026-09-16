using Avalonia.Controls;
using Epoxy;
using naget.Assets.Locales;
using naget.Helpers;

namespace naget.ViewModels;

[ViewModel]
public class AccessibilityPermissionWindowViewModel
{
	public Well<Window> WindowWell { get; } = Well.Factory.Create<Window>();

	public static string WindowTitle => Resources.Accessibility_Window_Title + " - " + App.ProductName;

	public bool IsPermissionGranted { get; private set; }
	public string StatusText { get; private set; } = string.Empty;

	public Command RequestPermissionCommand { get; }
	public Command OpenSystemSettingsCommand { get; }
	public Command RecheckCommand { get; }
	public Command RestartCommand { get; }
	public Command LaterCommand { get; }

	public AccessibilityPermissionWindowViewModel()
	{
		// ウィンドウがロードされた時の処理
		WindowWell.Add(Window.LoadedEvent, () =>
		{
			App.Logger.Debug("AccessibilityPermissionWindow Loaded");
			RefreshPermissionState();
			return default;
		});

		// アクセシビリティ API へのアクセス許可を要求するコマンド
		RequestPermissionCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Request Accessibility API access");
			HotKeyHelper.RequestAccessibilityApiAccess();
			RefreshPermissionState();
			return default;
		});

		// システム設定のアクセシビリティ画面を開くコマンド
		OpenSystemSettingsCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Open Accessibility System Settings");
			HotKeyHelper.OpenAccessibilitySystemSettings();
			return default;
		});

		// 権限の状態を再確認するコマンド
		RecheckCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Recheck Accessibility API access");
			RefreshPermissionState();
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
	}

	public void RefreshPermissionState()
	{
		bool isEnabled = HotKeyHelper.IsAccessibilityApiEnabled();

		// 権限が許可された場合はホットキーの登録を開始する
		if (isEnabled && !IsPermissionGranted) HotKeyHelper.Run();

		IsPermissionGranted = isEnabled;
		StatusText = IsPermissionGranted ? Resources.Accessibility_Status_Granted : Resources.Accessibility_Status_NotGranted;
	}
}
