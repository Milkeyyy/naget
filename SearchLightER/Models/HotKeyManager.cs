using Avalonia.Threading;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchLight.Models;

public class HotKeyGroup
{
	public readonly HashSet<KeyCode> Keys;
	public readonly Action Method;
	public HotKeyGroup(HashSet<KeyCode> keys, Action method)
	{
		Keys = keys;
		Method = method;
	}
}

public class HotKeyManager
{
	private TaskPoolGlobalHook hook;
	private HashSet<KeyCode> pressedKeys;
	private List<HotKeyGroup> groups;

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
		pressedKeys.Add(e.Data.KeyCode);
		foreach (var group in groups)
		{
			if (group.Keys.All(y => pressedKeys.Any(l => l == y)) && pressedKeys.All(y => group.Keys.Any(l => l == y)))
			{
				Dispatcher.UIThread.Invoke(group.Method);
			}
		}
	}

	private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
	{
		pressedKeys.Remove(e.Data.KeyCode);
	}

	/*
	public void AddKeys(KeyCode keyCode)
	{
		 _pressedKeys.Add(keyCode);
	}

	public void RemoveKeys(KeyCode keyCode)
	{
		_pressedKeys.Remove(keyCode);
	}
	*/
}
