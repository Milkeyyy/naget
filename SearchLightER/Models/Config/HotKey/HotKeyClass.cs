using SearchLight.Assets.Locales;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchLight.Models.Config.HotKey;

public class HotKeyGroup(string name, HashSet<KeyCode>? keys = null)
{
	/// <summary>
	/// ホットキー固有のID
	/// </summary>
	public string Id { get; set; } = Guid.NewGuid().ToString();
	/// <summary>
	/// ホットキーの名前
	/// </summary>
	public string Name { get; set; } = name;
	/// <summary>
	/// キーの一覧
	/// </summary>
	public HashSet<KeyCode>? Keys { get; set; } = keys;
	public string CommandId { get; set; } = string.Empty;
	public override string ToString()
	{
		if (Keys == null) return Resources.Settings_ShortcutKey_Preset_NotSet;
		return string.Join("+", Keys.Select(k => k.ToString()));
	}
}

/*public class HotKeyList
{
	public HotKeyList()
	{
		CreatedAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
		List = [];
	}
	public string CreatedAt { get; set; }
	public List<HotKeyGroup> List { get; set; }
}*/
