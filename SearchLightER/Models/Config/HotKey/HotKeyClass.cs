using SearchLight.Assets.Locales;
using SearchLight.Models.Config.HotKey.Action;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SearchLight.Models.Config.HotKey;

public class HotKeyGroup
{
	/// <summary>
	/// ホットキー固有のID
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// ホットキーの名前
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// キーの一覧
	/// </summary>
	public HashSet<KeyCode>? Keys { get; set; }

	/// <summary>
	/// アクションのクラス
	/// </summary>
	//[JsonIgnore]
	[JsonPropertyName("Action")]
	public HotKeyAction ActionObj { get; private set; }

	[JsonIgnore]
	public string ActionId { get { return ActionObj.Id; } set { ActionObj = HotKeyActionList.CreateInstance(value); } } // アクションのIDが変更されたらアクションを再生成

	[JsonIgnore]
	public Dictionary<string, string> ActionProperty => ActionObj.Property;

	/// <summary>
	/// アクション情報の辞書
	/// </summary>
	[JsonIgnore]
	//[JsonPropertyName("Action")]
	public Dictionary<string, object> ActionDict { get { return ActionObj.ToDictionary(); } }

	public HotKeyGroup(string name, HashSet<KeyCode>? keys = null)
	{
		Id = Guid.NewGuid().ToString();
		Name = name;
		Keys = keys;
		ActionObj = HotKeyActionList.GetActionById("None");
	}

	/// <summary>
	/// JSONデシリアライズ用コンストラクター
	/// </summary>
	/// <param name="Id"></param>
	/// <param name="Name"></param>
	/// <param name="Keys"></param>
	/// <param name="ActionObj"></param>
	[JsonConstructor]
	public HotKeyGroup(string Id, string Name, HashSet<KeyCode> Keys, HotKeyAction ActionObj)
	{
		this.Id = Id;
		this.Name = Name;
		this.Keys = Keys;
		this.ActionObj = ActionObj; //new(ActionDict.Id, ActionDict.Property);
	}

	public override string ToString()
	{
		if (Keys == null) return Resources.Settings_ShortcutKey_Preset_NotSet;
		return string.Join("+", Keys.Select(k => k.ToString()));
	}
}
