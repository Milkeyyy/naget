using SearchLight.Assets.Locales;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
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
	public HotKeyAction Action { get; private set; }

	[JsonIgnore]
	public HotKeyActionType ActionType
	{
		get { return Action.ActionType; }
		set { if (Action.ActionType != value) Action = new HotKeyAction(value); } // アクションのタイプが変更されたらアクションを再生成 (すでに同じアクションの場合は行わない)
	}

	[JsonIgnore]
	public Dictionary<string, string> ActionProperty => Action.Property;

	public HotKeyGroup(string name, HashSet<KeyCode>? keys = null)
	{
		Id = Guid.NewGuid().ToString();
		Name = name;
		Keys = keys;
		Action = new HotKeyAction(HotKeyActionType.None);
	}

	/// <summary>
	/// JSONデシリアライズ用コンストラクター
	/// </summary>
	/// <param name="Id"></param>
	/// <param name="Name"></param>
	/// <param name="Keys"></param>
	/// <param name="Action"></param>
	[JsonConstructor]
	public HotKeyGroup(string Id, string Name, HashSet<KeyCode> Keys, HotKeyAction Action)
	{
		this.Id = Id;
		this.Name = Name;
		this.Keys = Keys;
		this.Action = Action; //new(ActionDict.Id, ActionDict.Property);
	}

	public override string ToString()
	{
		if (Keys == null) return Resources.Settings_ShortcutKey_Preset_NotSet;
		return string.Join("+", Keys.Select(k => k.ToString()));
	}
}
