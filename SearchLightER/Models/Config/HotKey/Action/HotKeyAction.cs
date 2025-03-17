using FluentAvalonia.UI.Controls;
using SearchLight.Assets.Locales;
using SearchLight.Models.SearchEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace SearchLight.Models.Config.HotKey.Action;

public static class HotKeyActionList
{
	/// <summary>
	/// ホットキーアクションの一覧
	/// </summary>
	public static ReadOnlyCollection<HotKeyAction> Actions { get; } = new([
		new( // なし
			Resources.Settings_ShortcutKey_Action_None,
			Symbol.Cancel,
			"None"
		),
		new( // ウェブ検索
			Resources.Settings_ShortcutKey_Action_WebSearch,
			Symbol.Find,
			"WebSearch",
			new Dictionary<string, object>()
			{
				{ "SearchEngineId", SearchEngineManager.EngineList[0].Id }
			}
		)
	]);

	/// <summary>
	/// 指定されたキーに一致するホットキーアクションを返す
	/// </summary>
	public static HotKeyAction GetActionById(string id)
	{
		var actions = Actions;
		foreach (var action in actions)
		{
			if (action.Id == id)
			{
				return action;
			}
		}
		return actions[0];
	}
}

public class HotKeyAction
{
	/// <summary>
	/// アクションの名前
	/// </summary>
	[JsonIgnore]
	public string Name { get; init; }

	/// <summary>
	/// アクションのアイコン
	/// </summary>
	[JsonIgnore]
	public Symbol Icon { get; init; }

	/// <summary>
	/// アクションのID
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// アクションのプロパティ
	/// </summary>
	public Dictionary<string, object> Property { get; protected set; }

	public HotKeyAction(string name, Symbol icon, string id, Dictionary<string, object>? property = null)
	{
		Name = name;
		Icon = icon;
		Id = id;
		Property = property ?? [];
	}

	// JSONデシリアライズ用コンストラクター
	[JsonConstructor]
	public HotKeyAction(string Id, Dictionary<string, object> Property)
	{
		var b = HotKeyActionList.GetActionById(Id);
		this.Name = b.Name;
		this.Icon = b.Icon;
		this.Id = Id;
		this.Property = Property;
	}
}
