using Avalonia.Controls;
using Epoxy;
using FluentAvalonia.UI.Controls;
using SearchLight.Assets.Locales;
using SearchLight.Models.Config;
using SearchLight.Models.Config.HotKey;
using SearchLight.Views.Settings;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SearchLight.ViewModels.Settings;

[ViewModel]
public class ShortcutKeyViewModel
{
	public Well<UserControl> ShortcutKeyWell { get; } = Well.Factory.Create<UserControl>();

	#region プリセット関連
	/// <summary>
	/// プリセットの一覧
	/// </summary>
	public ReadOnlyCollection<HotKeyGroup>? PresetList { get; private set; }
	/// <summary>
	/// 選択中のプリセットのオブジェクト (HotKeyGroup)
	/// </summary>
	public HotKeyGroup? SelectedPresetItem { get; set; }
	/// <summary>
	/// 選択中のプリセットのインデックス
	/// </summary>
	public int PresetListSelectedIndex { get; set; }
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

	#region キー登録関連
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
	#endregion

	public Command ExitCommand { get; }

	public ShortcutKeyViewModel()
	{
		// プリセットの一覧を取得 登録されているプリセットの個数が0の場合はnull
		LoadPresetList();

		ShortcutKeyWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("ShortcutKeyView Loaded");
			RegisteredKeysText = string.Empty;
			KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			return default;
		});

		// プリセット作成コマンド
		PresetCreateCommand = Command.Factory.Create(() =>
		{
			Debug.WriteLine("Execute PresetCreateCommand");
			ShowInputDialogAsync();
			return default;
		});

		// キー登録コマンド
		KeyRegisterCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute KeyRegisterCommand");
			Debug.WriteLine("- KeyRegistrationMode: " + KeyRegistrationMode);
			if (KeyRegistrationMode)
			{
				// キー登録モードを終了する
				var result = ConfigManager.HotKeyManager.EndKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			}
			else
			{
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

		ExitCommand = Command.Factory.Create(() =>
		{
			Environment.Exit(0);
			return default;
		});
	}

	private void LoadPresetList()
	{
		if (ConfigManager.HotKeyManager.List.Count == 0)
		{
			PresetList = null;
		}
		else
		{
			PresetList = ConfigManager.HotKeyManager.List;
			PresetListSelectedIndex = 0;
		}
	}

	[PropertyChanged(nameof(KeyRegistrationMode))]
	private ValueTask KeyRegistrationModeChanged(bool value)
	{
		Debug.WriteLine("KeyRegistrationMode Changed: " + KeyRegistrationMode);
		KeyRegisterButtonText = value ? Resources.Settings_ShortcutKey_RegisterKeys_Done : Resources.Settings_ShortcutKey_RegisterKeys_Register;
		return default;
	}

	[PropertyChanged(nameof(SelectedPresetItem))]
	private ValueTask SelectedPresetItemChanged(HotKeyGroup? value)
	{
		Debug.WriteLine("SelectedPresetItem Changed: " + SelectedPresetName);
		// 選択されたプリセットに登録されているキーを表示する
		RegisteredKeysText = value?.ToString() ?? Resources.Settings_ShortcutKey_Preset_NotSet;
		return default;
	}

	public async void ShowInputDialogAsync()
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
