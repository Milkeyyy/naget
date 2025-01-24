using Avalonia.Threading;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SearchLight.Models.Config.HotKey;

public class HotKeyGroup
{
    public string Id { get; private set; }
    public readonly HashSet<KeyCode> Keys;
    public readonly Action? Method;
    public HotKeyGroup(HashSet<KeyCode> keys, Action? method = null)
    {
        Id = Guid.NewGuid().ToString();
        Keys = keys;
        Method = method;
    }
}

public class HotKeyManager
{
    private TaskPoolGlobalHook hook;
    private HashSet<KeyCode> pressedKeys;
    private List<HotKeyGroup> groups;

    private int keyRegsitrationMode;
    private HashSet<KeyCode> registrationQueuedKeys;

    public HotKeyManager()
    {
        hook = new TaskPoolGlobalHook();
        hook.KeyPressed += Hook_KeyPressed;
        hook.KeyReleased += Hook_KeyReleased;

        pressedKeys = [];

        groups = [];
    }

    public void Dispose()
    {
        hook.Dispose();
    }

    public void Register(HotKeyGroup group)
    {
        groups.Add(group);
    }

    public void Run()
    {
        var t = hook.RunAsync();
    }

    private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
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
        }

        pressedKeys.Add(e.Data.KeyCode);
        foreach (var group in groups)
        {
            if (group.Keys.All(y => pressedKeys.Any(l => l == y)) && pressedKeys.All(y => group.Keys.Any(l => l == y)))
            {
                if (group.Method != null) Dispatcher.UIThread.Invoke(group.Method);
            }
        }
    }

    private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        pressedKeys.Remove(e.Data.KeyCode);
    }

    private HotKeyGroup? _GetHotKeyGroupFromKey(string id)
    {
        return groups.FirstOrDefault(x => x.Id == id);
    }

    private HotKeyGroup _RegisterKeys(HashSet<KeyCode> keys)
    {
        var g = new HotKeyGroup(keys);
        groups.Add(g);
        Debug.WriteLine("Key registered: " + string.Join(", ", keys));
        return g;
    }

    private async Task<string?> _StartKeyRegistrationAsync()
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
                await Task.Delay(100);
            }

            // キー登録がキャンセルされた場合は空文字を返す
            if (keyRegsitrationMode == -1)
            {
                _CancelKeyRegistration();
                return string.Empty;
            }
            // キー登録が完了した場合は登録されたキーのIDを返す
            var r = _StopKeyRegistration();
            return r;
        }
    }

    private string? _StopKeyRegistration()
    {
        Debug.WriteLine("Key registration stopped");
        keyRegsitrationMode = 0;
        if (registrationQueuedKeys.Count == 0) return null;
        var g = _RegisterKeys(registrationQueuedKeys);
        return g.Id;
    }

    private bool _CancelKeyRegistration()
    {
        Debug.WriteLine("Key registration canceled");
        keyRegsitrationMode = 0;
        registrationQueuedKeys = [];
        return true;
    }

    public HotKeyGroup? GetHotKeyGroupFromKey(string id)
    {
        return _GetHotKeyGroupFromKey(id);
    }

    public async Task<string?> StartKeyRegistrationAsync()
    {
        return await _StartKeyRegistrationAsync();
    }

    public bool EndKeyRegistration()
    {
        if (keyRegsitrationMode != 1) return false;
        keyRegsitrationMode = 0;
        return true;
    }

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
