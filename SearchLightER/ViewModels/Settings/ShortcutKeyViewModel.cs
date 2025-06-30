using Avalonia.Controls;
using Epoxy;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Helpers;
using naget.Models.Config;
using naget.Models.Config.HotKey;
using naget.Models.SearchEngine;
using naget.Views.Dialog;
using naget.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class ShortcutKeyViewModel
{
	public Well<UserControl> ShortcutKeyWell { get; } = Well.Factory.Create<UserControl>();

	#region プリセット
	/// <summary>
	/// プリセットの一覧 (リストの一覧へバインドされるオブジェクト)
	/// </summary>
	public ReadOnlyCollection<HotKeyGroup> HotKeyPresetList { get; set; } = new([]);
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
	/// <summary>
	/// プリセット名称変更コマンド
	/// </summary>
	public Command PresetRenameCommand { get; }
	/// <summary>
	/// プリセット削除コマンド
	/// </summary>
	public Command PresetDeleteCommand { get; }
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
	/// キー登録ボタンのアイコン
	/// </summary>
	public string KeyRegisterButtonIcon { get; private set; } = "PlayFilled";
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
	/// <summary>
	/// キー登録キャンセルコマンド
	/// </summary>
	public Command KeyRegisterCancelCommand { get; }
	#endregion

	#region ホットキーアクション
	/// <summary>
	/// アクションの一覧 (リストの一覧へバインドされるオブジェクト)
	/// </summary>
	public static ReadOnlyCollection<HotKeyAction> HotKeyActionList { get { return HotKeyAction.Actions; } }
	/// <summary>
	/// 選択中のアクションのオブジェクト (リストの選択項目へバインドされるオブジェクト)
	/// </summary>
	public HotKeyAction? SelectedActionItem { get; set; }
	/// <summary>
	/// 選択中のアクションのタイプ
	/// </summary>
	public HotKeyActionType SelectedActionType {
		get { return SelectedActionItem.ActionType; }
		set { SelectedActionItem = HotKeyAction.GetActionByType(value); } // アクションのタイプが変更されたら選択中のアクションのオブジェクトも変更する
	}
	/// <summary>
	/// 選択中のアクションのインデックス
	/// </summary>
	public int SelectedActionIndex { get; set; }
	/// <summary>
	/// 選択中のプリセットのアクション
	/// </summary>
	public HotKeyAction? PresetAction { get { return SelectedPresetItem?.Action; } }
	#endregion

	#region アクション: 検索エンジン
	/// <summary>
	/// 検索エンジンの一覧 (リストの一覧へバインドされるオブジェクト)
	/// </summary>
	public static ReadOnlyCollection<SearchEngineClass> SearchEngineList { get { return SearchEngineManager.EngineList; } }
	/// <summary>
	/// 選択中の検索エンジン (リストの選択項目へバインドされるオブジェクト)
	/// </summary>
	public SearchEngineClass? SelectedSearchEngineItem { get; set; }
	/// <summary>
	/// 選択中の検索エンジンのインデックス
	/// </summary>
	public int SearchEngineListSelectedIndex { get; set; }
	/// <summary>
	/// 検索エンジンのリストを表示するかどうか
	/// </summary>
	public bool SearchEngineListIsVisible { get; set; }
	#endregion

	public bool ViewIsLoaded { get; set; }
	public bool HotKeyPresetListLoaded { get; set; }
	public bool HotKeyPresetLoaded { get; set; }
	public bool HotKeyActionLoaded { get; set; }
	public bool HotKeyActionSearchEngineLoaded { get; set; }

	public Command ExitCommand { get; }

	public ShortcutKeyViewModel()
	{
		// ビューがロードされた時の処理
		ShortcutKeyWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("ShortcutKeyView Loaded");

			RegisteredKeysText = string.Empty;
			KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			KeyRegisterButtonIcon = "PlayFilled";

			ViewIsLoaded = true;

			// プリセット等を読み込む 登録されているプリセットの個数が0の場合はnull
			LoadPresetList();

			/*HotKeyPresetListLoaded = true;

			SelectedPresetIndex = 0;
			if (SelectedPresetItem != null) SelectedActionItem = HotKeyAction.GetActionByType(SelectedPresetItem.Action.ActionType);*/

			return default;
		});
		// ビューがアンロードされた時の処理
		ShortcutKeyWell.Add(Control.UnloadedEvent, () =>
		{
			Debug.WriteLine("ShortcutKeyView Unloaded");

			// キーの登録をキャンセルする
			HotKeyHelper.CancelKeyRegistration();

			return default;
		});

		// プリセット作成コマンド
		PresetCreateCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute PresetCreateCommand");
			await ShowInputDialogAsync();
		});

		// プリセット名称変更コマンド
		PresetRenameCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute SelectedPresetItem");
			await ShowPresetRenameDialogAsync();
		});

		// プリセット削除コマンド
		PresetDeleteCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute PresetDeleteCommand");
			await ShowPresetDeleteDialogAsync();
		});

		// キー登録コマンド
		KeyRegisterCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute KeyRegisterCommand");
			Debug.WriteLine("- KeyRegistrationMode: " + KeyRegistrationMode);
			if (KeyRegistrationMode)
			{
				Debug.WriteLine("End Key Registration");
				// キー登録モードを終了する
				var result = HotKeyHelper.EndKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
				KeyRegisterButtonIcon = "PlayFilled";
			}
			else
			{
				Debug.WriteLine("Start Key Registration");
				// キー登録モードに入る
				KeyRegistrationMode = true;
				// キーが押されるたびに押された(登録される)キーを表示する
				var progress = new Progress<string>(keys =>
				{
					RegisteredKeysText = keys;
				});
				// キー登録を開始する
				await HotKeyHelper.StartKeyRegistrationAsync(SelectedPresetId, progress);
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
				var result = HotKeyHelper.EndKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
				KeyRegisterButtonIcon = "PlayFilled";
			}
			return default;
		});

		// キー登録キャンセルコマンド
		KeyRegisterCancelCommand = Command.Factory.Create(() =>
		{
			Debug.WriteLine("Execute KeyRegisterCancelCommand");
			Debug.WriteLine("- KeyRegistrationMode: " + KeyRegistrationMode);
			if (KeyRegistrationMode)
			{
				Debug.WriteLine("Cancel Key Registraion");
				var result = HotKeyHelper.CancelKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
				KeyRegisterButtonIcon = "PlayFilled";
				// 現在登録されているキーを表示し直す
				RegisteredKeysText = SelectedPresetItem?.ToString() ?? Resources.Settings_ShortcutKey_Preset_NotSet;
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
		HotKeyPresetList = ConfigManager.HotKeyManager.List;

		HotKeyPresetListLoaded = true;

		// すべての選択リストを初期化
		SelectedPresetIndex = -1;
		SelectedActionIndex = -1;
		SearchEngineListSelectedIndex = -1;
		SelectedPresetItem = null;
		SelectedActionItem = null;
		SelectedSearchEngineItem = null;

		// プリセット一覧が1つ以上の場合は選択リストのインデックスを0に設定する
		if (HotKeyPresetList.Count > 0)
		{
			SelectedPresetIndex = 0;
		}
	}

	private void SaveValue()
	{
		Debug.WriteLine("Save Value");

		if (HotKeyPresetList.Count == 0 || SelectedPresetIndex == -1) return;

		var action = HotKeyPresetList[SelectedPresetIndex].Action;

		// 選択されたプリセットのアクションを設定する
		ConfigManager.HotKeyManager.List[SelectedPresetIndex].ActionType = SelectedActionItem.ActionType;
		// アクションがウェブ検索の場合は検索エンジンを設定されているものにする
		if (action.ActionType == HotKeyActionType.WebSearch)
		{
			ConfigManager.HotKeyManager.List[SelectedPresetIndex].Action.Property["SearchEngineId"] = SelectedSearchEngineItem.Id;
		}
	}

	private void LoadSearchEngine()
	{
		if (SelectedPresetItem == null) return;

		Debug.WriteLine("Load Search Engine");
		
		// 検索エンジンを読み込む
		string eid = SelectedPresetItem.Action.Property.GetValueOrDefault(
			"SearchEngineId",
			SearchEngineManager.GetDefaultEngine().Id
		);
		
		Debug.WriteLine(" - SearchEngineId: " + eid);
		
		var se = SearchEngineManager.Get(eid);
		if (se != null)
		{
			Debug.WriteLine(" - SearchEngine is not null");
			SelectedSearchEngineItem = se;
		}
		else
		{
			Debug.WriteLine(" - SearchEngine is null");
			SelectedSearchEngineItem = SearchEngineList[0];
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
		// キー登録モードが有効になった場合はキー登録ボタンのテキストを変更する
		if (value)
		{
			KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Done;
			KeyRegisterButtonIcon = "Accept";
		}
		else
		{
			KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			KeyRegisterButtonIcon = "PlayFilled";
		}
		// 登録されているホットキーを更新する
		if (HotKeyPresetList != null) RegisteredKeysText = HotKeyPresetList[SelectedPresetIndex].ToString();
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

		KeyRegisterCancelCommand.Execute(null);

		if (value == null)
		{
			Debug.WriteLine("SelectedPresetItem is null");
			return default;
		}

		HotKeyPresetLoaded = false;

		if (!ViewIsLoaded || !HotKeyPresetListLoaded || value == null) return default;

		// 選択されたプリセットに登録されているキーを表示する
		RegisteredKeysText = value?.ToString() ?? Resources.Settings_ShortcutKey_Preset_NotSet;

		Debug.WriteLine("- Execute");

		// アクションの選択リストをプリセットのアクションにする
		if (value.Action == null) SelectedActionType = HotKeyActionList[0].ActionType;
		else SelectedActionType = value.ActionType;

		// 選択されたアクションがウェブ検索の場合は検索エンジンのリストを表示する
		SearchEngineListIsVisible = value.Action?.ActionType == HotKeyActionType.WebSearch;

		// アクションがウェブ検索の場合は検索エンジンを設定されているものにする
		if (value.ActionType == HotKeyActionType.WebSearch)
		{
			Debug.WriteLine(" - WebSearch");
			LoadSearchEngine();
		}

		HotKeyPresetLoaded = true;
		
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
		Debug.WriteLine("SelectedActionItem Changed: " + value?.Name);

		if (!ViewIsLoaded || !HotKeyPresetListLoaded || value == null) return default;

		HotKeyActionLoaded = false;

		if (HotKeyPresetList?.Count != 0 && SelectedPresetItem != null && SearchEngineList != null)
		{
			Debug.WriteLine("- Execute");

			// 選択されたアクションをプリセットに設定する
			SelectedPresetItem.ActionType = value.ActionType;

			// 選択されたアクションがウェブ検索の場合は検索エンジンのリストを表示する
			SearchEngineListIsVisible = SelectedPresetItem.Action.ActionType == HotKeyActionType.WebSearch;

			// アクションがウェブ検索の場合
			if (value.ActionType == HotKeyActionType.WebSearch)
			{
				Debug.WriteLine("- WebSearch");
				LoadSearchEngine();
			}

			HotKeyActionLoaded = true;
		}
		SaveValue();
		return default;
	}

	[PropertyChanged(nameof(SearchEngineListIsVisible))]
	private ValueTask SearchEngineListIsVisibleChanged(bool value)
	{
		Debug.WriteLine("SearchEngineListIsVisible Changed: " + value);
		HotKeyActionSearchEngineLoaded = false;
		return default;
	}

	/// <summary>
	/// 選択された検索エンジンが変更された時の処理
	/// </summary>
	[PropertyChanged(nameof(SelectedSearchEngineItem))]
	private ValueTask SelectedSearchEngineItemChanged(SearchEngineClass value)
	{
		Debug.WriteLine("SelectedSearchEngineItem Changed: " + value?.Id);

		if (!ViewIsLoaded || !HotKeyPresetListLoaded || value == null) return default;

		if (HotKeyPresetList?.Count != 0 && SelectedPresetItem != null && SearchEngineList != null)
		{
			// 選択された検索エンジンをアクションに設定する
			if (SelectedActionType == HotKeyActionType.WebSearch)
			{
				SelectedPresetItem.Action.Property["SearchEngineId"] = value.Id;
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

	public async Task ShowPresetRenameDialogAsync()
	{
		if (SelectedPresetItem == null) return;

		string? result = await SuperDialog.Input(Resources.Settings_ShortcutKey_Preset_Dialog_Rename_Title, Resources.Settings_ShortcutKey_Preset_Dialog_Rename_InputTitle);

		// キャンセルボタンが押された場合
		if (result == null) return;

		// 名前が入力されていない場合
		if (string.IsNullOrWhiteSpace(result))
		{
			await SuperDialog.Info(App.SettingsWindow, Resources.Settings_ShortcutKey_Preset_Dialog_Rename_Title, Resources.Settings_ShortcutKey_Preset_Dialog_Rename_NameIsEmpty);
			await ShowPresetRenameDialogAsync();
			return;
		}

		// 名称変更
		ConfigManager.HotKeyManager.RenameGroup(SelectedPresetItem.Id, result);
		// プリセットの一覧を更新
		LoadPresetList();
	}

	private static readonly CompositeFormat deleteDialogContentDesc = CompositeFormat.Parse(Resources.Settings_ShortcutKey_Preset_Dialog_Delete_Description);
	public async Task ShowPresetDeleteDialogAsync()
	{
		if (SelectedPresetItem == null) return;

		var dialog = new ContentDialog
		{
			// 説明
			Content = string.Format(null, deleteDialogContentDesc, SelectedPresetItem.Name),

			// タイトル
			Title = Resources.Settings_ShortcutKey_Preset_Dialog_Delete_Title,

			// ボタンのテキスト
			IsSecondaryButtonEnabled = false, // 第二ボタンを無効化
			PrimaryButtonText = Resources.Settings_ShortcutKey_Preset_Dialog_Delete_Confirm,
			CloseButtonText = Resources.Strings_Cancel,
		};

		// ダイアログを表示する
		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			Debug.WriteLine("Preset Delete Dialog - User clicked Delete");

			// プリセットを削除する
			ConfigManager.HotKeyManager.DeleteGroup(SelectedPresetItem.Id);

			// プリセットを読み込み直す
			LoadPresetList();
		}
		else
		{
			Debug.WriteLine("Preset Delete Dialog - User clicked Cancel");
		}
	}
}
