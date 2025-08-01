﻿using Avalonia.Controls;
using Epoxy;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Models.SearchEngine;
using naget.Views.Dialog;
using naget.Views.Settings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class SearchViewModel
{
	public Well<UserControl> ViewWell { get; } = Well.Factory.Create<UserControl>();

	public ReadOnlyCollection<SearchEngineViewModel> SearchEngineList { get; private set; }

	public Command CreateSearchEngineCommand { get; }
	public Command ReloadSearchEngineCommand { get; }

	public SearchViewModel()
	{
		SearchEngineList = new([]);

		// ビューがロードされた時の処理
		ViewWell.Add(Control.LoadedEvent, () =>
		{
			App.Logger.Debug("SearchView Loaded");
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
			App.Logger.Debug("Delete Search Engine Command");
			LoadSearchEngineList();
			return default;
		});
	}

	public void LoadSearchEngineList()
	{
		App.Logger.Debug("Load Search Engine List");
		// 検索エンジンのリストを初期化する
		SearchEngineList = new(SearchEngineManager.EngineList.Select(v => new SearchEngineViewModel(v, ReloadSearchEngineCommand)).ToList());
	}

	[PropertyChanged(nameof(SearchEngineList))]
	private ValueTask SearchEngineListChangedAsync(ReadOnlyCollection<SearchEngineClass> value)
	{
	 	App.Logger.Debug("Search Engine List Changed");
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
		};

		// ダイアログを表示する
		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			App.Logger.Debug("Search Engine Create Dialog - User clicked Create");
			if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.Url))
			{
				App.Logger.Debug("Search Engine Create Dialog - Preset Name or URL is empty");
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
			App.Logger.Debug("Search Engine Create Dialog - User clicked Cancel");
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
			App.Logger.Debug($"Edit Search Engine: {Name} ({Id})");
			await ShowEditDialogAsync();
		});

		// 検索エンジンの削除コマンド
		DeleteSearchEngineCommand = Command.Factory.Create(async () =>
		{
			App.Logger.Debug($"Delete Search Engine: {Name} ({Id})");
			await ShowDeleteDialogAsync();
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
		};

		// ダイアログのデータコンテキストに現在の検索エンジンの情報を設定する
		vm.Name = Name;
		vm.Url = Uri;

		// ダイアログを表示する
		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			App.Logger.Debug("Search Engine Create Dialog - User clicked Create");
			if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.Url))
			{
				App.Logger.Debug("Search Engine Create Dialog - Preset Name or URL is empty");
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
			App.Logger.Debug("Search Engine Create Dialog - User clicked Cancel");
		}
	}

	private static readonly CompositeFormat deleteDialogContentDesc = CompositeFormat.Parse(Resources.Settings_Search_SearchEngine_Dialog_Delete_Description);
	public async Task ShowDeleteDialogAsync()
	{
		var dialog = new ContentDialog
		{
			// 説明
			Content = string.Format(null, deleteDialogContentDesc, Name),

			// タイトル
			Title = Resources.Settings_Search_SearchEngine_Dialog_Delete_Title,

			// ボタンのテキスト
			IsSecondaryButtonEnabled = false, // 第二ボタンを無効化
			PrimaryButtonText = Resources.Settings_Search_SearchEngine_Dialog_Delete_Confirm,
			CloseButtonText = Resources.Strings_Cancel,
		};

		// ダイアログを表示する
		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			App.Logger.Debug("Search Engine Delete Dialog - User clicked Delete");

			// 検索エンジンを削除する
			var r = SearchEngineManager.Delete(Id);

			// 削除できなかった場合はダイアログで通知する
			if (!r)
			{
				await SuperDialog.Info(
				App.SettingsWindow,
				Resources.Settings_Search_SearchEngine_Dialog_Delete_CannotDelete_Title,
				Resources.Settings_Search_SearchEngine_Dialog_Delete_CannotDelete_Description
			);
				return;
			}

			// 検索エンジン一覧を読み込み直す
			ReloadSearchEngineCommand.Execute(null);
		}
		else
		{
			App.Logger.Debug("Search Engine Delete Dialog - User clicked Cancel");
		}
	}
}
