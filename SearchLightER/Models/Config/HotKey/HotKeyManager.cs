using SharpHook.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace naget.Models.Config.HotKey;

public class HotKeyManager
{
	/// <summary>
	/// ホットキーの一覧 (内部)
	/// </summary>
	public List<HotKeyGroup> Groups { get; set; }
	/// <summary>
	/// ホットキーの一覧
	/// </summary>
	public ReadOnlyCollection<HotKeyGroup> List => new(Groups);

	public HotKeyManager()
	{
		Groups = [];
	}

	public void LoadGroups(List<HotKeyGroup> groups)
	{
		Debug.WriteLine("Group loaded: " + groups.Count);
		Groups = groups;
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
	public bool DeleteGroup(string id)
	{
		int result = Groups.RemoveAll(x => x.Id == id);
		return result > 0;
	}

	public bool RenameGroup(string id, string name)
	{
		HotKeyGroup? g = GetHotKeyGroupFromKey(id);
		if (g == null) return false;
		g.Name = name;
		return true;
	}

	private HotKeyGroup? _GetHotKeyGroupFromKey(string id)
	{
		return Groups.FirstOrDefault(x => x.Id == id);
	}

	/// <summary>
	/// 指定されたIDのグループにキーを登録する
	/// </summary>
	/// <param name="groupId"></param>
	/// <param name="keys"></param>
	/// <returns></returns>
	public HotKeyGroup RegisterKeys(string groupId, HashSet<KeyCode> keys)
	{
		// 渡されたIDからホットキーグループを取得する
		var g = _GetHotKeyGroupFromKey(groupId);
		// 取得したグループのキーに渡されたキーを設定する
		g.Keys = keys;
		Debug.WriteLine($"Key registered: {groupId} | " + string.Join(", ", keys));
		return g;
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
}
