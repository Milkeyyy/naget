using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Models.SearchEngine;
using naget.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace naget.Models.Config.HotKey;

/*public static class HotKeyActionList
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
			new Dictionary<string, string>()
			{
				{ "SearchEngineId", SearchEngineManager.EngineList[0].Id }
			}
		)
	]);

	/// <summary>
	/// ホットキーアクションのID一覧
	/// </summary>
	public static ReadOnlyCollection<string> ActionIds { get; } = new(Actions.Select(x => x.Id).ToList());

	/// <summary>
	/// 指定されたキーに一致するホットキーアクションを返す
	/// </summary>
	public static HotKeyAction GetActionById(string id)
	{
		foreach (var action in Actions)
		{
			if (action.Id == id)
			{
				return action;
			}
		}
		return Actions[0];
	}

	/// <summary>
	/// 指定されたIDのホットキーアクションのインスタンスを新規作成する
	/// </summary>
	public static HotKeyAction CreateInstance(string id)
	{
		return new HotKeyAction(id, GetActionById(id).Property.ToDictionary());
	}
}*/

public enum HotKeyActionType
{
	None, // なし
	WebSearch // ウェブ検索
}

public class HotKeyAction
{
	private readonly static ReadOnlyCollection<HotKeyAction> ActionList;
	public readonly static ReadOnlyCollection<HotKeyAction> Actions;

	// 静的コンストラクターで直接インスタンスを作成するための内部コンストラクター
	private HotKeyAction(HotKeyActionType type, string name, Symbol icon, Dictionary<string, string> property)
	{
		ActionType = type;
		Name = name;
		Icon = icon;
		Property = property;
	}

	static HotKeyAction()
	{
		App.Logger.Debug("HotKeyAction static constructor");
		// 各アクションのプロパティを明示的に渡すことで、通常のコンストラクターを経由しない
		/*var noneAction = new HotKeyAction(
			HotKeyActionType.None,
			Resources.Settings_ShortcutKey_Action_None,
			Symbol.Cancel,
			new Dictionary<string, string>()
		);
		var webSearchProps = new Dictionary<string, string>()
			{
				{ "SearchEngineId", SearchEngineManager.EngineList[0].Id }
			};
		var webSearchAction = new HotKeyAction(
			HotKeyActionType.WebSearch,
			Resources.Settings_ShortcutKey_Action_WebSearch,
			Symbol.Find,
			webSearchProps
		);*/
		ActionList = new([
			new HotKeyAction(
				HotKeyActionType.None,
				Resources.Settings_ShortcutKey_Action_None,
				Symbol.Cancel,
				[]
			),
			new HotKeyAction(
				HotKeyActionType.WebSearch,
				Resources.Settings_ShortcutKey_Action_WebSearch,
				Symbol.Find,
				new Dictionary<string, string>()
				{
					{ "SearchEngineId", SearchEngineManager.EngineList[0].Id }
				}
			)
		]);
		Actions = new(ActionList);
	}

	// 以降は従来のメソッド・プロパティ定義を維持
	public static HotKeyAction GetActionByType(HotKeyActionType type)
	{
		App.Logger.Debug("GetActionByType");
		foreach (var action in ActionList)
		{
			if (action.ActionType == type)
			{
				return action;
			}
		}
		return ActionList[0];
	}

	private static HotKeyAction GetActionCloneByType(HotKeyActionType type)
	{
		App.Logger.Debug("GetActionCloneByType");
		return (HotKeyAction)GetActionByType(type).MemberwiseClone();
	}

	[JsonIgnore]
	public string Name { get; init; }
	[JsonIgnore]
	public Symbol Icon { get; init; }
	public HotKeyActionType ActionType { get; init; }
	[JsonIgnore]
	public string Id { get { return ActionType.ToString(); } }
	public Dictionary<string, string> Property { get; protected set; }

	/*public HotKeyAction(HotKeyActionType type, string name, Symbol icon, Dictionary<string, string>? property = null)
	{
		ActionType = type;
		Name = name;
		Icon = icon;
		Property = property ?? GetActionCloneByType(type).Property;
	}*/

	public HotKeyAction(HotKeyActionType type)
	{
		HotKeyAction a = GetActionCloneByType(type);
		ActionType = a.ActionType;
		Name = a.Name;
		Icon = a.Icon;
		Property = a.Property;
	}

	/// <summary>
	/// JSONデシリアライズ用コンストラクター
	/// </summary>
	[JsonConstructor]
	public HotKeyAction(HotKeyActionType actionType, Dictionary<string, string> property)
	{
		var b = new HotKeyAction(actionType);
		this.Name = b.Name;
		this.Icon = b.Icon;
		this.ActionType = actionType;
		this.Property = property;
	}

	public Dictionary<string, object> ToDictionary()
	{
		return new() {
				{ "Id", Id },
				{ "Property", Property }
			};
	}

	public void Action()
	{
		switch (ActionType)
		{
			case HotKeyActionType.WebSearch:
				Dispatcher.UIThread.Invoke(() => (App.MainWindow.DataContext as MainWindowViewModel).Search(Property["SearchEngineId"]));
				break;
		}
	}
}
