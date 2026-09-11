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
		App.Logger.Debug("Group loaded: " + groups.Count);
		Groups = groups;
		/*foreach (var group in groups)
		{
			App.Logger.Debug(group.Id);
			App.Logger.Debug("- " + group.Name);
			App.Logger.Debug("- " + group.Action);
			App.Logger.Debug("- " + group);
		}*/
	}

	/// <summary>
	/// 新しいグループを作成する
	/// </summary>
	/// <param name="name">グループの名前</param>
	/// <param name="key"></param>
	/// <param name="modifiers"></param>
	public void CreateGroup(string name, KeyCode key = KeyCode.VcUndefined, KeyModifiers modifiers = KeyModifiers.None)
	{
		Groups.Add(new HotKeyGroup(name, key, modifiers));
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
	/// <param name="key"></param>
	/// <param name="modifiers"></param>
	/// <returns></returns>
	public HotKeyGroup RegisterKeys(string groupId, KeyCode key, KeyModifiers modifiers)
	{
		// 渡されたIDからホットキーグループを取得する
		HotKeyGroup g = _GetHotKeyGroupFromKey(groupId)!;
		// 取得したグループのキーに渡されたキーを設定する
		g.Key = key;
		g.Modifiers = modifiers;
		App.Logger.Debug($"Key registered: {groupId} | " + g.ToString());
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
