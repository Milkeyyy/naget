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
	private static readonly TaskPoolGlobalHook hook;
	private static readonly object pressedKeysLock;
	/// <summary>
	/// 押されているキーの一覧
	/// </summary>
	private static readonly HashSet<KeyCode> pressedKeys;
	/// <summary>
	/// 押されているキーの文字列表現
	/// </summary>
	private static string PressedKeysString { get { return string.Join(";", pressedKeys.Select(k => k.ToString())); } }
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
	/// <summary>
	/// 登録候補のキーの一覧
	/// </summary>
	private static HashSet<KeyCode> registrationQueuedKeys;
	private static readonly object registrationQueuedKeysLock;

	static HotKeyHelper()
	{
		hook = new TaskPoolGlobalHook(
			globalHookType: GlobalHookType.Keyboard, // グローバルフックのタイプをキーボードに設定
			runAsyncOnBackgroundThread: true // バックグラウンドスレッドで実行する
		);
		hook.KeyPressed += Hook_KeyPressed;
		hook.KeyReleased += Hook_KeyReleased;

		pressedKeys = [];
		pressedKeysLock = new();

		registrationMode = HotKeyRegistrationMode.None;
		registrationGroupId = string.Empty;
		registrationQueuedKeys = [];
		registrationQueuedKeysLock = new();
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
		int beforePressedKeysCount = pressedKeys.Count;

		lock (pressedKeysLock)
		{
			//Debug.WriteLine("Key pressed: " + e.Data.KeyCode);
			pressedKeys.Add(e.Data.KeyCode);
		}
		
		if (registrationMode == HotKeyRegistrationMode.Registering)
		{
			if (e.Data.KeyCode == KeyCode.VcEscape)
			{
				// クリアのみロック
				lock (registrationQueuedKeysLock) { registrationQueuedKeys.Clear(); }
				// ロック外でキャンセル
				CancelKeyRegistration();
				return;
			}
			// 追加のみロック
			lock (registrationQueuedKeysLock)
			{
				// キーが押されていない状態から新たに押された場合はリストをクリアする (新たに登録を開始する)
				if (beforePressedKeysCount == 0)
				{
					Debug.WriteLine("Key registration started: " + registrationGroupId);
					registrationQueuedKeys.Clear();
				}
				var r = registrationQueuedKeys.Add(e.Data.KeyCode);
				Debug.WriteLine($"Key added: {e.Data.KeyCode} ({r})");
			}
			return;
		}

		foreach (var group in Groups)
		{
			if (group?.Keys != null)
			{
				if (group.Keys.All(y => pressedKeys.Any(l => l == y)) && pressedKeys.All(y => group.Keys.Any(l => l == y)))
				{
					e.SuppressEvent = true;
					group.Action.Action();
					Debug.WriteLine("HotKey pressed: " + group.Name);
				}
			}
		}
	}

	/// <summary>
	/// キーが離された時のイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private static void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
	{
		lock (pressedKeysLock)
		{
			//Debug.WriteLine("Key released: " + e.Data.KeyCode);
			pressedKeys.Remove(e.Data.KeyCode);
		}
	}

	public static async Task<bool> StartKeyRegistrationAsync(string groupId, IProgress<string>? progress = null)
	{
		// キー登録が既に行われている場合は何もしない
		if (registrationMode == HotKeyRegistrationMode.Registering) return false;

		Debug.WriteLine($"Key registration started: {groupId}");

		string regKeyText = string.Empty;

		registrationGroupId = groupId; // 登録する対象のグループのIDを設定
		registrationQueuedKeys = []; // 登録候補のキーの一覧を初期化
		registrationMode = HotKeyRegistrationMode.Registering;

		while (registrationMode == HotKeyRegistrationMode.Registering)
		{
			lock (registrationQueuedKeysLock)
			{
				// キーが何も押されていない場合は Esc を押してキャンセル という表示にする
				if (registrationQueuedKeys.Count == 0)
				{
					progress?.Report(Resources.Settings_ShortcutKey_RegisterKeys_PressEscToCancel);
				}
				else
				{
					// キーが押されている場合は登録候補のキーをUIに表示する
					regKeyText = string.Join(" + ", registrationQueuedKeys.Select(k => KeyCodeName.Get(k)));
					progress?.Report(regKeyText);
				}
			}
			await Task.Delay(10);
		}

		// キー登録がキャンセルされた場合
		if (registrationMode == HotKeyRegistrationMode.Canceled)
		{
			Debug.WriteLine("Key registration canceled");
			EndKeyRegistration();

			// キー登録モードをリセット
			registrationMode = HotKeyRegistrationMode.None;

			return false;
		}

		// 渡されたIDに該当するグループにキーを登録する
		lock (registrationQueuedKeysLock)
		{
			var g = ConfigManager.HotKeyManager.RegisterKeys(registrationGroupId, registrationQueuedKeys);
			// キーの登録を終了
			var r = EndKeyRegistration();
			// 新たに登録されたキーをUIに表示する
			progress?.Report(g.ToString());
		}

		return true;
	}

	/// <summary>
	/// アロー↑リ
	/// </summary>
	public static void DaolerKeyRegistration()
	{
		lock (registrationQueuedKeysLock)
		{
			registrationQueuedKeys.Clear();
		}
	}

	public static bool EndKeyRegistration()
	{
		if (registrationMode == HotKeyRegistrationMode.None || registrationMode == HotKeyRegistrationMode.Canceled) return false;
		registrationMode = HotKeyRegistrationMode.None;
		return true;
	}

	public static bool CancelKeyRegistration()
	{
		if (registrationMode == HotKeyRegistrationMode.None || registrationMode == HotKeyRegistrationMode.Canceled) return false;
		registrationMode = HotKeyRegistrationMode.Canceled;
		return true;
	}
}

public enum HotKeyRegistrationMode
{
	None = 0,
	Registering = 1,
	Daoler = 2,
	Canceled = -1
}
