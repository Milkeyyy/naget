using SharpHook.Native;
using System;
using System.Collections.Generic;

namespace SearchLight.Models.Config.HotKey;

public class HotKeyGroup(HashSet<KeyCode> keys)
{
	/// <summary>
	/// ホットキー固有のID
	/// </summary>
	public string Id { get; private set; } = Guid.NewGuid().ToString();
	/// <summary>
	/// キーの一覧
	/// </summary>
	public HashSet<KeyCode> Keys { get; } = keys;
	public string CommandId { get; set; } = string.Empty;
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
