using Avalonia.Controls;
using Epoxy;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Models.Config;
using naget.Models.SearchEngine;
using naget.Views;
using naget.Views.Settings;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class SearchViewModel
{
	public Well<UserControl> SearchViewWell { get; } = Well.Factory.Create<UserControl>();
	
	public static ReadOnlyCollection<SearchEngineClass> SearchEngineList { get { return SearchEngineManager.EngineList; } }

	public Command AddSearchEngineCommand { get; }

	public SearchViewModel()
	{
		// ビューがロードされた時の処理
		SearchViewWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("SearchView Loaded");
			Debug.WriteLine(SearchEngineList);
			return default;
		});

		AddSearchEngineCommand = Command.Factory.Create(async () =>
		{
			await ShowInputDialogAsync();
		});
	}

	public async Task ShowInputDialogAsync()
	{
		var vm = new SearchEngineDialogContentViewModel();
		var dialog = new ContentDialog
		{
			// 作成画面
			Content = new SearchEngineDialogContent
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
			if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.Url))
			{
				Debug.WriteLine("HotKey Preset Create Dialog - Preset Name or URL is empty");
				await SuperDialog.Info(App.SettingsWindow, Resources.Settings_ShortcutKey_Preset_CreateNewPreset, Resources.Settings_ShortcutKey_Preset_NameIsEmpty);
				await ShowInputDialogAsync();
				return;
			}
			// プリセットを作成する
			SearchEngineManager.Create(vm.Name, vm.Url);
		}
		else
		{
			Debug.WriteLine("HotKey Preset Create Dialog - User clicked Cancel");
		}
	}
}
