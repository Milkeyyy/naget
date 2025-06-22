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
using System.Linq;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class SearchViewModel
{
	public Well<UserControl> SearchViewWell { get; } = Well.Factory.Create<UserControl>();

	public ReadOnlyCollection<SearchEngineViewModel> SearchEngineList { get; private set; }

	public Command CreateSearchEngineCommand { get; }
	public Command ReloadSearchEngineCommand { get; }

	public SearchViewModel()
	{
		SearchEngineList = new([]);

		// ビューがロードされた時の処理
		SearchViewWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("SearchView Loaded");
			Debug.WriteLine(SearchEngineList);
			LoadSearchEngineList();
			return default;
		});

		// 検索エンジンの作成コマンド
		CreateSearchEngineCommand = Command.Factory.Create(async () =>
		{
			await ShowInputDialogAsync();
		});

		// 検索エンジンの削除コマンド
		ReloadSearchEngineCommand = Command.Factory.Create(() =>
		{
			Debug.WriteLine("Delete Search Engine Command");
			LoadSearchEngineList();
			return default;
		});
	}

	public void LoadSearchEngineList()
	{
		Debug.WriteLine("Load Search Engine List");
		// 検索エンジンのリストを初期化する
		SearchEngineList = new(SearchEngineManager.EngineList.Select(v => new SearchEngineViewModel(v, ReloadSearchEngineCommand)).ToList());
	}

	[PropertyChanged(nameof(SearchEngineList))]
	private ValueTask SearchEngineListChangedAsync(ReadOnlyCollection<SearchEngineClass> value)
	{
	 	Debug.WriteLine("Search Engine List Changed");
		// 検索エンジン一覧を読み込み直す
		LoadSearchEngineList();
		return default;
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
			Title = Resources.Settings_Search_SearchEngine_Dialog_Add,

			// ボタンのテキスト
			IsSecondaryButtonEnabled = false, // 第二ボタンを無効化
			PrimaryButtonText = Resources.Strings_Ok,
			CloseButtonText = Resources.Strings_Cancel,

			// 作成ボタンが押された時の処理
			//PrimaryButtonCommand = command
		};

		// ダイアログを表示する
		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			Debug.WriteLine("Search Engine Create Dialog - User clicked Create");
			if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.Url))
			{
				Debug.WriteLine("Search Engine Create Dialog - Preset Name or URL is empty");
				await SuperDialog.Info(App.SettingsWindow, Resources.Settings_Search_SearchEngine_Dialog_Add, Resources.Settings_ShortcutKey_Preset_NameIsEmpty);
				await ShowInputDialogAsync();
				return;
			}
			// プリセットを作成する
			SearchEngineManager.Create(vm.Name, vm.Url);

			// 検索エンジン一覧を読み込み直す
			LoadSearchEngineList();
		}
		else
		{
			Debug.WriteLine("Search Engine Create Dialog - User clicked Cancel");
		}
	}
}

[ViewModel]
public class SearchEngineViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Uri { get; set; } = string.Empty;

	public Command EditSearchEngineCommand { get; }
	public Command DeleteSearchEngineCommand { get; }
	public Command ReloadSearchEngineCommand { get; }

	public SearchEngineViewModel(SearchEngineClass searchEngine, Command reloadCommand)
	{
		Id = searchEngine.Id;
		Name = searchEngine.Name;
		Uri = searchEngine.Uri;
		ReloadSearchEngineCommand = reloadCommand;

		// 検索エンジンの編集コマンド
		EditSearchEngineCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine($"Edit Search Engine: {Name} ({Id})");
			await ShowEditDialogAsync();
		});

		// 検索エンジンの削除コマンド
		DeleteSearchEngineCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine($"Delete Search Engine: {Name} ({Id})");
			SearchEngineManager.Delete(Id);
			ReloadSearchEngineCommand.Execute(null);
		});
	}

	public async Task ShowEditDialogAsync()
	{
		var vm = new SearchEngineDialogContentViewModel();
		var dialog = new ContentDialog
		{
			// 作成画面
			Content = new SearchEngineDialogContent
			{ DataContext = vm },

			// タイトル
			Title = Resources.Settings_Search_SearchEngine_Dialog_Edit,

			// ボタンのテキスト
			IsSecondaryButtonEnabled = false, // 第二ボタンを無効化
			PrimaryButtonText = Resources.Strings_Ok,
			CloseButtonText = Resources.Strings_Cancel,

			// 作成ボタンが押された時の処理
			//PrimaryButtonCommand = command
		};

		// ダイアログのデータコンテキストに現在の検索エンジンの情報を設定する
		vm.Name = Name;
		vm.Url = Uri;

		// ダイアログを表示する
		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			Debug.WriteLine("Search Engine Create Dialog - User clicked Create");
			if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.Url))
			{
				Debug.WriteLine("Search Engine Create Dialog - Preset Name or URL is empty");
				await SuperDialog.Info(App.SettingsWindow, Resources.Settings_Search_SearchEngine_Dialog_Edit, Resources.Settings_ShortcutKey_Preset_NameIsEmpty);
				await ShowEditDialogAsync();
				return;
			}

			// 検索エンジンの情報を更新する
			SearchEngineManager.Rename(Id, vm.Name);
			SearchEngineManager.UpdateUri(Id, vm.Url);

			// 検索エンジン一覧を読み込み直す
			ReloadSearchEngineCommand.Execute(null);
		}
		else
		{
			Debug.WriteLine("Search Engine Create Dialog - User clicked Cancel");
		}
	}
}
