using SearchLight.Assets.Locales;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SearchLight.Models.Config.HotKey;

public class HotKeyManager : IDisposable
{
	/// <summary>
	/// キーのグローバルフック
	/// </summary>
	private readonly SimpleGlobalHook hook;
	/// <summary>
	/// 押されているキーの一覧
	/// </summary>
	private readonly HashSet<KeyCode> pressedKeys;
	/// <summary>
	/// ホットキーの一覧 (内部)
	/// </summary>
	public List<HotKeyGroup> Groups { get; set; }
	/// <summary>
	/// ホットキーの一覧
	/// </summary>
	public ReadOnlyCollection<HotKeyGroup> List => new(Groups);

	private int keyRegsitrationMode;
	private HashSet<KeyCode> registrationQueuedKeys;

	public HotKeyManager()
	{
		hook = new SimpleGlobalHook();
		hook.KeyPressed += Hook_KeyPressed;
		hook.KeyReleased += Hook_KeyReleased;

		Groups = [];

		pressedKeys = [];

		registrationQueuedKeys = [];
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private bool _disposed;
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing) hook.Dispose();
			_disposed = true;
		}
	}

	public void LoadGroups(List<HotKeyGroup> groups)
	{
		Debug.WriteLine("Group loaded: " + groups.Count);
		this.Groups = groups;
		/*foreach (var group in groups)
		{
			Debug.WriteLine(group.Id);
			Debug.WriteLine("- " + group.Name);
			Debug.WriteLine("- " + group.Action);
			Debug.WriteLine("- " + group);
		}*/
	}

	/// <summary>
	/// 新しいグループを作成する
	/// </summary>
	/// <param name="name">グループの名前</param>
	/// <param name="keys">キーの一覧</param>
	public void CreateGroup(string name, HashSet<KeyCode>? keys = null)
	{
		Groups.Add(new HotKeyGroup(name, keys));
	}

	/// <summary>
	/// 指定されたIDのグループを削除する
	/// </summary>
	/// <param name="id">削除する対象のID</param>
	public void DeleteGroup(string id)
	{
		Groups.RemoveAll(x => x.Id == id);
	}

	public void Run()
	{
		var t = hook.RunAsync();
	}

	private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
	{
		//Debug.WriteLine("Key pressed: " + e.Data.KeyCode);
		// キー登録モードの場合は押されたキーを登録するキー一覧へ追加する
		if (keyRegsitrationMode == 1)
		{
			// Escキーが押されたらキー登録をキャンセルする
			if (e.Data.KeyCode == KeyCode.VcEscape)
			{
				registrationQueuedKeys = [];
				var t = CancelKeyRegistration();
				return;
			}
			Debug.WriteLine("Key added: " + e.Data.KeyCode);
			registrationQueuedKeys.Add(e.Data.KeyCode);
			return;
		}

		pressedKeys.Add(e.Data.KeyCode);
		foreach (var group in Groups)
		{
			if (group != null)
			{
				if (group.Keys == null) continue;
				if (group.Keys.All(y => pressedKeys.Any(l => l == y)) && pressedKeys.All(y => group.Keys.Any(l => l == y)))
				{
					Debug.WriteLine("HotKey pressed: " + group.Name);
					e.SuppressEvent = true;
					group.Action.Action();
				}
			}
		}
	}

	private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
	{
		pressedKeys.Remove(e.Data.KeyCode);
	}

	private HotKeyGroup? _GetHotKeyGroupFromKey(string id)
	{
		return Groups.FirstOrDefault(x => x.Id == id);
	}

	private HotKeyGroup _RegisterKeys(string groupId, HashSet<KeyCode> keys)
	{
		// 渡されたIDからホットキーグループを取得する
		var g = _GetHotKeyGroupFromKey(groupId);
		// 取得したグループのキーに渡されたキーを設定する
		g.Keys = keys;
		Debug.WriteLine("Key registered: " + string.Join(", ", keys));
		return g;
	}

	private async Task<string?> _StartKeyRegistrationAsync(string groupId, IProgress<string>? progress = null)
	{
		if (keyRegsitrationMode == 1) return null;
		else
		{
			Debug.WriteLine("Key registration started");
			keyRegsitrationMode = 1;
			registrationQueuedKeys = [];

			while (keyRegsitrationMode == 1)
			{
				if (keyRegsitrationMode == -1) break;
				// キーが何も押されていない場合は Esc を押してキャンセル という表示にする 1つでも押されている場合は押されたキーをUIに表示する
				if (registrationQueuedKeys.Count == 0) progress?.Report(Resources.Settings_ShortcutKey_RegisterKeys_PressEscToCancel);
				else progress?.Report(string.Join("+", registrationQueuedKeys.Select(k => k.ToString())));
				await Task.Delay(10);
			}

			// キー登録がキャンセルされた場合は空文字を返す
			if (keyRegsitrationMode == -1)
			{
				_CancelKeyRegistration();
				progress?.Report(string.Empty);
				return string.Empty;
			}
			// キー登録が完了した場合は登録されたキーのIDを返す
			var r = _StopKeyRegistration(groupId);
			var regKeys = GetHotKeyGroupFromKey(r)?.ToString(); // 返されたIDからホットキーを取得する
			// 登録されたキーをUIに表示する
			if (regKeys != null) progress?.Report(regKeys);
			else if (r == null) progress?.Report(string.Empty);
			return r;
		}
	}

	private string? _StopKeyRegistration(string groupId)
	{
		Debug.WriteLine("Key registration stopped");
		keyRegsitrationMode = 0;
		if (registrationQueuedKeys.Count == 0) return null;
		var g = _RegisterKeys(groupId, registrationQueuedKeys);
		return g.Id;
	}

	private bool _CancelKeyRegistration()
	{
		Debug.WriteLine("Key registration canceled");
		keyRegsitrationMode = 0;
		registrationQueuedKeys = [];
		return true;
	}

	/// <summary>
	/// 指定されたIDに一致するホットキーグループを取得する
	/// </summary>
	/// <param name="id">対象のID</param>
	/// <returns>ホットキーグループ 見つからなかった場合は null</returns>
	public HotKeyGroup? GetHotKeyGroupFromKey(string id)
	{
		return _GetHotKeyGroupFromKey(id);
	}

	/// <summary>
	/// ホットキーの登録を開始する
	/// </summary>
	/// <param name="groupId">登録する対象のグループのID</param>
	/// <param name="progress">進捗状況</param>
	/// <returns>登録されたホットキーグループのID</returns>
	public async Task<string?> StartKeyRegistrationAsync(string groupId, IProgress<string>? progress = null)
	{
		return await _StartKeyRegistrationAsync(groupId, progress);
	}

	/// <summary>
	/// ホットキーの登録を終了する
	/// </summary>
	/// <returns>終了できた場合は true 既に登録が終了されている場合は false</returns>
	public bool EndKeyRegistration()
	{
		if (keyRegsitrationMode != 1) return false;
		keyRegsitrationMode = 0;
		return true;
	}

	/// <summary>
	/// ホットキーの登録をキャンセルする
	/// </summary>
	/// <returns>終了できた場合は true 既に登録が終了されている場合は false</returns>
	public bool CancelKeyRegistration()
	{
		if (keyRegsitrationMode != 1) return false;
		keyRegsitrationMode = -1;
		return true;
	}

	/*
	public void RemoveKeys(KeyCode keyCode)
	{
		_pressedKeys.Remove(keyCode);
	}
	*/
}
