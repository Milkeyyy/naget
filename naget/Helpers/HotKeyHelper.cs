using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using naget.Assets.Locales;
using naget.Models.Config;
using naget.Models.Config.HotKey;
using SharpHook;
using SharpHook.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace naget.Helpers;

public static class HotKeyHelper
{
	/// <summary>
	/// キーのグローバルフック
	/// </summary>
	private static readonly SimpleGlobalHook hook;
	private static readonly object pressedKeysLock;

	private static KeyModifiers currentModifiers;

	/// <summary>
	/// ホットキーの一覧 (内部)
	/// </summary>
	public static List<HotKeyGroup> Groups => ConfigManager.HotKeyManager.Groups;
	/// <summary>
	/// ホットキーの一覧
	/// </summary>
	public static ReadOnlyCollection<HotKeyGroup> List => new(Groups);

	/// <summary>
	/// キー登録モード
	/// </summary>
	private static HotKeyRegistrationMode registrationMode;
	/// <summary>
	/// キーを登録する対象のグループのID
	/// </summary>
	private static string registrationGroupId;

	// 検索ウィンドウをマウスカーソルがあるウィンドウへ表示するためにマウスカーソルの座標を取得
	/// <summary>
	/// マウスカーソルの座標
	/// </summary>
	public static System.Drawing.Point MousePointerCoordinates { get; private set; } = new(0, 0);

	static HotKeyHelper()
	{
		hook = new SimpleGlobalHook(
			globalHookType: GlobalHookType.All, // グローバルフックのタイプをキーボード+マウスに設定
			runAsyncOnBackgroundThread: true // バックグラウンドスレッドで実行する
		);
		hook.KeyPressed += Hook_KeyPressed;
		hook.KeyReleased += Hook_KeyReleased;
		hook.MouseMoved += Hook_MouseMoved;

		pressedKeysLock = new();
		currentModifiers = KeyModifiers.None;

		registrationMode = HotKeyRegistrationMode.None;
		registrationGroupId = string.Empty;
	}

	public static void Run()
	{
		var t = hook.RunAsync();
	}

	public static void Stop()
	{
		hook.Dispose();
	}

	/// <summary>
	/// キーが押された時のイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private static void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
	{
		try
		{
			// KeyCode to Modifiers
			if (IsModifier(e.Data.KeyCode, out var modifier))
			{
				currentModifiers |= modifier;
			}

			if (registrationMode == HotKeyRegistrationMode.Registering)
			{
				if (e.Data.KeyCode == KeyCode.VcEscape)
				{
					// キャンセル
					CancelKeyRegistration();
					return;
				}

				// 修飾キーのみの場合は無視して表示だけ更新する (後続処理で押されたキーとして扱わない)
				if (IsModifier(e.Data.KeyCode, out _))
				{
					// UI更新はメインループ側で行われるため、ここでは状態更新のみ
					return;
				}

				// 修飾キー以外のキーが押されたら登録完了
				lock (pressedKeysLock)
				{
					App.Logger.Debug($"Key registered: {e.Data.KeyCode} + {currentModifiers}");

					// 登録完了処理は非同期タスク側で検知させるか、ここで完了させる
					// ここでは完了フラグを立てるのではなく、登録処理を呼び出す
					// ただし StartKeyRegistrationAsync はループしているので、そこで状態を見る形にする
					// ここでは何もしなくて良い、ループ側で状態を見るため
				}
				return;
			}

			// ホットキー判定
			// 修飾キー単体の場合は発火させない
			if (IsModifier(e.Data.KeyCode, out _)) return;

			foreach (var group in Groups)
			{
				if (group.Key == e.Data.KeyCode && group.Modifiers == currentModifiers)
				{
					e.SuppressEvent = true;
					group.Action.Action();
					App.Logger.Debug("HotKey pressed: " + group.Name);
				}
			}
		}
		catch (Exception ex)
		{
			App.Logger.Error("HotKey Pressed Event Error: " + ex.Message);
		}
	}

	/// <summary>
	/// キーが離された時のイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private static void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
	{
		try
		{
			if (IsModifier(e.Data.KeyCode, out var modifier))
			{
				currentModifiers &= ~modifier;
			}
		}
		catch (Exception ex)
		{
			App.Logger.Error("HotKey Released Event Error: " + ex.Message);
		}
	}

	private static bool IsModifier(KeyCode key, out KeyModifiers modifier)
	{
		modifier = KeyModifiers.None;
		switch (key)
		{
			case KeyCode.VcLeftControl:
			case KeyCode.VcRightControl:
				modifier = KeyModifiers.Control;
				return true;
			case KeyCode.VcLeftShift:
			case KeyCode.VcRightShift:
				modifier = KeyModifiers.Shift;
				return true;
			case KeyCode.VcLeftAlt:
			case KeyCode.VcRightAlt:
				modifier = KeyModifiers.Alt;
				return true;
			case KeyCode.VcLeftMeta:
			case KeyCode.VcRightMeta:
				modifier = KeyModifiers.Meta;
				return true;
		}
		return false;
	}

	/// <summary>
	/// マウスが動いた時のイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <exception cref="NotImplementedException"></exception>
	private static void Hook_MouseMoved(object? sender, MouseHookEventArgs e)
	{
		// マウスポインターの座標を更新
		MousePointerCoordinates = new(e.Data.X, e.Data.Y);
	}

	public static async Task<bool> StartKeyRegistrationAsync(string groupId, IProgress<string>? progress = null)
	{
		// キー登録が既に行われている場合は何もしない
		if (registrationMode == HotKeyRegistrationMode.Registering) return false;

		App.Logger.Debug($"Key registration started: {groupId}");

		registrationGroupId = groupId; // 登録する対象のグループのIDを設定
		registrationMode = HotKeyRegistrationMode.Registering;
		currentModifiers = KeyModifiers.None; // 登録開始時に修飾キー状態をリセット (念のため) Or keep it? keeping it is better logic wise if user is already holding it. 
											  // しかし GlobalHook なので状態は常に最新であるはず。

		KeyCode pressedKey = KeyCode.VcUndefined;
		KeyModifiers pressedModifiers = KeyModifiers.None;

		// イベントハンドラを追加して、修飾キー以外のキーが押されるのを待つ
		// Hook_KeyPressed で登録完了ロジックを書くのが難しいので、ここでループ検知するか、
		// Hook_KeyPressed からイベントを飛ばすか。
		// シンプルに、Hook_KeyPressed で「最後に押された非修飾キー」を記録し、それを拾う形にする。

		KeyCode detectedKey = KeyCode.VcUndefined;

		void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
		{
			if (registrationMode != HotKeyRegistrationMode.Registering) return;
			if (e.Data.KeyCode == KeyCode.VcEscape) return; // Hook側で処理
			if (!IsModifier(e.Data.KeyCode, out _))
			{
				detectedKey = e.Data.KeyCode;
				pressedModifiers = currentModifiers;
			}
		}

		hook.KeyPressed += OnKeyPressed;

		try
		{
			while (registrationMode == HotKeyRegistrationMode.Registering)
			{
				// UI更新
				var textParts = new List<string>();
				if (currentModifiers.HasFlag(KeyModifiers.Control)) textParts.Add("Ctrl");
				if (currentModifiers.HasFlag(KeyModifiers.Alt)) textParts.Add("Alt");
				if (currentModifiers.HasFlag(KeyModifiers.Shift)) textParts.Add("Shift");
				if (currentModifiers.HasFlag(KeyModifiers.Meta)) textParts.Add("Meta");

				if (detectedKey != KeyCode.VcUndefined)
				{
					// キーが検知されたらループを抜ける
					pressedKey = detectedKey;
					break;
				}

				if (textParts.Count == 0)
				{
					progress?.Report(Resources.Settings_ShortcutKey_RegisterKeys_PressEscToCancel);
				}
				else
				{
					progress?.Report(string.Join(" + ", textParts) + " + ...");
				}

				await Task.Delay(10);
			}
		}
		finally
		{
			hook.KeyPressed -= OnKeyPressed;
		}

		// キー登録がキャンセルされた場合
		if (registrationMode == HotKeyRegistrationMode.Canceled)
		{
			EndKeyRegistration();
			// キー登録モードをリセット
			registrationMode = HotKeyRegistrationMode.None;
			return false;
		}

		// 渡されたIDに該当するグループにキーを登録する
		var g = ConfigManager.HotKeyManager.RegisterKeys(registrationGroupId, pressedKey, pressedModifiers);
		// キーの登録を終了
		EndKeyRegistration();
		// 新たに登録されたキーをUIに表示する
		progress?.Report(g.ToString());

		return true;
	}

	public static bool EndKeyRegistration()
	{
		if (registrationMode == HotKeyRegistrationMode.None || registrationMode == HotKeyRegistrationMode.Canceled) return false;
		App.Logger.Debug("Key registration ended");
		registrationMode = HotKeyRegistrationMode.None;
		return true;
	}

	public static bool CancelKeyRegistration()
	{
		if (registrationMode == HotKeyRegistrationMode.None || registrationMode == HotKeyRegistrationMode.Canceled) return false;
		App.Logger.Debug("Key registration canceled");
		registrationMode = HotKeyRegistrationMode.Canceled;
		return true;
	}

	/// <summary>
	/// 指定したウィンドウの大きさを元にマウスカーソルが存在するディスプレイの中央の座標を計算して返す
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static PixelPoint? GetCenterScreen(Window window)
	{
		App.Logger.Debug($"GetCenterScreen ({window})");

		// ウィンドウのサイズが設定されていない場合は null を返す
		if (double.IsNaN(window.Width) || double.IsNaN(window.Height))
		{
			return null;
		}

		// マウスカーソルの座標からディスプレイを取得
		Screen? screen = window.Screens.ScreenFromPoint(
			new(
				MousePointerCoordinates.X,
				MousePointerCoordinates.Y
			)
		);

		App.Logger.Debug($"-  Pos: {window.Position}");
		App.Logger.Debug($"- Size: {window.Width},{window.Height}");

		// ディスプレイを取得できた場合は中央の位置を計算して返す
		if (screen != null)
		{
			var screenCenterPos = screen.WorkingArea.Center;
			App.Logger.Debug($"Center Pos: {screenCenterPos.X},{screenCenterPos.Y}");
			return new(
				(int)(screenCenterPos.X - (window.Width / 2)),
				(int)(screenCenterPos.Y - (window.Height / 2))
			);
		}
		return null;
	}
}

public enum HotKeyRegistrationMode
{
	None = 0,
	Registering = 1,
	Canceled = -1
}
