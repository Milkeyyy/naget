using Avalonia.Controls;
using Epoxy;
using FluentAvalonia.UI.Controls;
using SearchLight.Assets.Locales;
using SearchLight.Models.Config;
using SearchLight.Models.Config.HotKey;
using SearchLight.Models.Config.HotKey.Action;
using SearchLight.Models.SearchEngine;
using SearchLight.Views;
using SearchLight.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SearchLight.ViewModels.Settings;

[ViewModel]
public class ShortcutKeyViewModel
{
	public Well<UserControl> ShortcutKeyWell { get; } = Well.Factory.Create<UserControl>();

	#region プリセット
	/// <summary>
	/// プリセットの一覧 (リストの一覧へバインドされるオブジェクト)
	/// </summary>
	public List<HotKeyGroup> HotKeyPresetList { get; set; } = [];
	/// <summary>
	/// 選択中のプリセットのオブジェクト (HotKeyGroup) (リストの選択項目へバインドされるオブジェクト)
	/// </summary>
	public HotKeyGroup? SelectedPresetItem { get; set; }
	/// <summary>
	/// 選択中のプリセットのインデックス
	/// </summary>
	public int SelectedPresetIndex { get; set; }
	/// <summary>
	/// 選択中のプリセットの名前
	/// </summary>
	public string SelectedPresetName { get { if (SelectedPresetItem != null) return SelectedPresetItem.Name; else return string.Empty; } }
	/// <summary>
	/// 選択中のプリセットのID
	/// </summary>
	public string SelectedPresetId { get { if (SelectedPresetItem != null) return SelectedPresetItem.Id; else return string.Empty; } }
	/// <summary>
	/// プリセット作成コマンド
	/// </summary>
	public Command PresetCreateCommand { get; }
	#endregion

	#region キー登録
	/// <summary>
	/// 登録されたキーのテキスト
	/// </summary>
	public string RegisteredKeysText { get; private set; } = string.Empty;
	/// <summary>
	/// キー登録ボタンのテキスト
	/// </summary>
	public string KeyRegisterButtonText { get; private set; } = Resources.Settings_ShortcutKey_RegisterKeys_Register;
	/// <summary>
	/// キー登録モードかどうか
	/// </summary>
	public bool KeyRegistrationMode { get; private set; }
	/// <summary>
	/// キー登録コマンド
	/// </summary>
	public Command KeyRegisterCommand { get; }
	/// <summary>
	/// キー登録終了コマンド
	/// </summary>
	public Command KeyRegisterEndCommand { get; }
	#endregion

	#region ホットキーアクション
	/// <summary>
	/// アクションの一覧 (リストの一覧へバインドされるオブジェクト)
	/// </summary>
	public static ReadOnlyCollection<HotKeyAction> HotKeyActionList { get { return Models.Config.HotKey.Action.HotKeyActionList.Actions; } }
	/// <summary>
	/// 選択中のアクションのオブジェクト (リストの選択項目へバインドされるオブジェクト)
	/// </summary>
	public HotKeyAction SelectedActionItem { get; set; }
	/// <summary>
	/// 選択中のアクションのID
	/// </summary>
	public string SelectedActionId {
		get { return SelectedActionItem.Id; }
		set { SelectedActionItem = Models.Config.HotKey.Action.HotKeyActionList.GetActionById(value); } // アクションのIDが変更されたら選択中のアクションのオブジェクトも変更する
	}
	/// <summary>
	/// 選択中のアクションの名前
	/// </summary>
	public string SelectedActionName { get { return SelectedActionItem.Name; } }
	/// <summary>
	/// 選択中のアクションのアイコン
	/// </summary>
	public Symbol SelectedActionIcon { get { return SelectedActionItem.Icon; } }
	/// <summary>
	/// 選択中のアクションのインデックス
	/// </summary>
	public int SelectedActionIndex { get; set; }
	#endregion

	#region アクション: 検索エンジン
	/// <summary>
	/// 検索エンジンの一覧 (リストの一覧へバインドされるオブジェクト)
	/// </summary>
	public ReadOnlyCollection<SearchEngineClass> SearchEngineList { get; private set; }
	/// <summary>
	/// 選択中の検索エンジン (リストの選択項目へバインドされるオブジェクト)
	/// </summary>
	public SearchEngineClass SelectedSearchEngineItem { get; set; }
	/// <summary>
	/// 選択中の検索エンジンのインデックス
	/// </summary>
	public int SearchEngineListSelectedIndex { get; set; }
	/// <summary>
	/// 検索エンジンのリストを表示するかどうか
	/// </summary>
	public bool SearchEngineListIsVisible { get; set; }
	#endregion

	public Command ExitCommand { get; }

	public ShortcutKeyViewModel()
	{
		ShortcutKeyWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("ShortcutKeyView Loaded");

			RegisteredKeysText = string.Empty;
			KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			
			// プリセット等を読み込む 登録されているプリセットの個数が0の場合はnull
			LoadPresetList();
			return default;
		});

		// プリセット作成コマンド
		PresetCreateCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute PresetCreateCommand");
			await ShowInputDialogAsync();
		});

		// キー登録コマンド
		KeyRegisterCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute KeyRegisterCommand");
			Debug.WriteLine("- KeyRegistrationMode: " + KeyRegistrationMode);
			if (KeyRegistrationMode)
			{
				Debug.WriteLine("End Key Registraion");
				// キー登録モードを終了する
				var result = ConfigManager.HotKeyManager.EndKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			}
			else
			{
				Debug.WriteLine("Start Key Registraion");
				// キー登録モードに入る
				KeyRegistrationMode = true;
				var progress = new Progress<string>(keys =>
				{
					RegisteredKeysText = keys;
				});
				var result = await ConfigManager.HotKeyManager.StartKeyRegistrationAsync(SelectedPresetId, progress);
				// キー登録が完了した場合は登録されたキーを表示する
				if (result != string.Empty && result != null)
				{
					var keys = ConfigManager.HotKeyManager.GetHotKeyGroupFromKey(result);
					RegisteredKeysText = keys?.ToString() ?? string.Empty;
				}
				KeyRegistrationMode = false;
			}
		});

		// キー登録終了コマンド
		KeyRegisterEndCommand = Command.Factory.Create(() =>
		{
			Debug.WriteLine("Execute KeyRegisterEndCommand");
			Debug.WriteLine("- KeyRegistrationMode: " + KeyRegistrationMode);
			if (KeyRegistrationMode)
			{
				Debug.WriteLine("End Key Registraion");
				var result = ConfigManager.HotKeyManager.EndKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			}
			return default;
		});

		ExitCommand = Command.Factory.Create(() =>
		{
			Environment.Exit(0);
			return default;
		});
	}

	private void LoadPresetList()
	{
		Debug.WriteLine("Load Preset List");

		// プリセットの一覧を取得	
		HotKeyPresetList = ConfigManager.Config.HotKeys;

		if (HotKeyPresetList.Count == 0) return;
		
		RegisteredKeysText = HotKeyPresetList[SelectedPresetIndex].ToString();
		
		// アクションの一覧を取得
		//HotKeyActionList = Models.Config.HotKey.Action.HotKeyActionList.Actions.ToList();
		//SelectedActionIndex = 0;
		
		// 検索エンジンの一覧を取得
		SearchEngineList = SearchEngineManager.EngineList;
		//SearchEngineListSelectedIndex = 0;

		// 選択中のプリセットをリセット
		SelectedPresetIndex = 0;
	}

	private void LoadActionList()
	{
		//SelectedActionItem = HotKeyAction.Get();
	}

	private void SaveValue()
	{
		Debug.WriteLine("Save Value");

		if (HotKeyPresetList.Count == 0) return;

		var action = HotKeyPresetList[SelectedPresetIndex].ActionObj;

		// 選択されたプリセットのアクションを設定する
		HotKeyPresetList[SelectedPresetIndex].ActionId = SelectedActionItem.Id;
		// アクションがウェブ検索の場合は検索エンジンを設定されているものにする
		if (action.Id == "WebSearch")
		{
			HotKeyPresetList[SelectedPresetIndex].ActionProperty["SearchEngineId"] = SelectedSearchEngineItem.Id;
		}
	}

	/// <summary>
	/// キー登録モードが変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(KeyRegistrationMode))]
	private ValueTask KeyRegistrationModeChanged(bool value)
	{
		Debug.WriteLine("KeyRegistrationMode Changed: " + KeyRegistrationMode);
		KeyRegisterButtonText = value ? Resources.Settings_ShortcutKey_RegisterKeys_Done : Resources.Settings_ShortcutKey_RegisterKeys_Register;
		return default;
	}

	/// <summary>
	/// 選択されたプリセットが変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(SelectedPresetItem))]
	private ValueTask SelectedPresetItemChanged(HotKeyGroup? value)
	{
		Debug.WriteLine("SelectedPresetItem Changed: " + SelectedPresetName);
		// 選択されたプリセットに登録されているキーを表示する
		RegisteredKeysText = value?.ToString() ?? Resources.Settings_ShortcutKey_Preset_NotSet;
		if (value != null)
		{
			Debug.WriteLine("- Execute");

			// アクションの選択リストをプリセットのアクションにする
			if (value.ActionObj == null) SelectedActionId = HotKeyActionList[0].Id;
			else SelectedActionId = value.ActionId;

			// アクションがウェブ検索の場合は検索エンジンを設定されているものにする
			if (SelectedActionId == "WebSearch")
			{
				// 検索エンジンを読み込む
				var eid = (string)value.ActionProperty.GetValueOrDefault(
					"SearchEngineId",
					SearchEngineManager.GetDefaultEngine().Id
				).ToString();
				SelectedSearchEngineItem = SearchEngineManager.Get(eid) ?? SearchEngineList[0];
			}
			// 選択されたアクションがウェブ検索の場合は検索エンジンのリストを表示する
			SearchEngineListIsVisible = value.ActionObj?.Id == "WebSearch";
		}
		SaveValue();
		return default;
	}

	/// <summary>
	/// 選択されたアクションが変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(SelectedActionItem))]
	private ValueTask SelectedActionItemChanged(HotKeyAction? value)
	{
		Debug.WriteLine("SelectedActionItem Changed: " + value);
		if (value != null && HotKeyPresetList?.Count != 0 && SelectedPresetItem != null && SearchEngineList != null)
		{
			Debug.WriteLine("- Execute");

			// 選択されたアクションをプリセットに設定する
			SelectedPresetItem.ActionId = value.Id;
			// アクションがウェブ検索の場合
			if (value.Id == "WebSearch")
			{
				// 検索エンジンを読み込む
				var eid = (string)SelectedPresetItem.ActionProperty.GetValueOrDefault(
					"SearchEngineId",
					SearchEngineManager.GetDefaultEngine().Id
				).ToString();
				SelectedSearchEngineItem = SearchEngineManager.Get(eid) ?? SearchEngineList[0];
			}
			// 選択されたアクションがウェブ検索の場合は検索エンジンのリストを表示する
			SearchEngineListIsVisible = value.Id == "WebSearch";
		}
		SaveValue();
		return default;
	}

	/// <summary>
	/// 選択された検索エンジンが変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(SelectedSearchEngineItem))]
	private ValueTask SelectedSearchEngineItemChanged(SearchEngineClass value)
	{
		Debug.WriteLine("SelectedSearchEngineItem Changed: " + value?.Id);
		if (value != null && HotKeyPresetList?.Count != 0 && SelectedPresetItem != null && SearchEngineList != null)
		{
			// 選択された検索エンジンをアクションに設定する
			if (SelectedActionId == "WebSearch")
			{
				SelectedPresetItem.ActionProperty["SearchEngineId"] = value.Id;
			}
			SaveValue();
		}
		return default;
	}

	public async Task ShowInputDialogAsync()
	{
		var vm = new ShortcutKeyPresetCreatorViewModel();
		var dialog = new ContentDialog
		{
			// 作成画面
			Content = new ShortcutKeyPresetCreator
			{ DataContext = vm },

			// タイトル
			Title = Resources.Settings_ShortcutKey_Preset_CreateNewPreset,

			// ボタンのテキスト
			IsSecondaryButtonEnabled = false, // 第二ボタンを無効化
			PrimaryButtonText = Resources.Strings_Ok,
			CloseButtonText = Resources.Strings_Cancel,
			
			// 作成ボタンが押された時の処理
			//PrimaryButtonCommand = command
		};

		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			Debug.WriteLine("HotKey Preset Create Dialog - User clicked Create");
			if (string.IsNullOrWhiteSpace(vm.PresetName))
			{
				Debug.WriteLine("HotKey Preset Create Dialog - Preset Name is empty");
				await SuperDialog.Info(App.SettingsWindow, Resources.Settings_ShortcutKey_Preset_CreateNewPreset, Resources.Settings_ShortcutKey_Preset_NameIsEmpty);
				await ShowInputDialogAsync();
				return;
			}
			// プリセットを作成する
			ConfigManager.HotKeyManager.CreateGroup(vm.PresetName);
			// プリセットの一覧を更新
			LoadPresetList();
		}
		else
		{
			Debug.WriteLine("HotKey Preset Create Dialog - User clicked Cancel");
		}
	}
}
