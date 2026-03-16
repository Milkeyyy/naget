using Epoxy;
using naget.Common;
using naget.Helpers;
using naget.Models.SearchEngine;
using System;

namespace naget.ViewModels;

[ViewModel]
public class MainWindowViewModel
{
	public string ProductNameText { get; } = App.ProductName;
	public string ProductVersionText { get; } = App.ProductVersion;
	public string SearchWord { get; set; } = string.Empty;

	// 選択中の検索エンジン
	private SearchEngineClass? _currentSearchEngine;
	private string? _currentSearchEngineId;
	public string? CurrentSearchEngineId // ID
	{
		get { return _currentSearchEngine?.Id; }
		set { _currentSearchEngineId = value; }
	}
	private string? _currentSearchEngineName;
	public string? CurrentSearchEngineName { get; set; }

	// 検索実行コマンド
	public Command SearchCommand { get; }

	/// <summary>
	/// ブラウザーを開いて検索を実行する
	/// </summary>
	private void DoSearch()
	{
		// ブラウザーを表示して検索結果を開く
		App.WindowService.ShowBrowser(string.Format(_currentSearchEngine.Uri, SearchWord));
		// 検索画面を閉じる
		App.WindowService.HideMainWindow();
	}

	/// <summary>
	/// 指定されたIDの検索エンジンを使用して検索を実行する
	/// </summary>
	/// <param name="EngineId">対象となる検索エンジンのID</param>
	public void Search(string? EngineId)
	{
		// 検索エンジンを読み込む
		if (EngineId != null)
		{
			var engine = SearchEngineManager.Get(EngineId);
			_currentSearchEngine = engine ?? SearchEngineManager.EngineList[0];
		}
		else
		{
			_currentSearchEngine = SearchEngineManager.EngineList[0];
		}
		// 検索エンジンの名前を更新
		CurrentSearchEngineName = _currentSearchEngine.Name;
		// 検索テキストの内容を消す
		SearchWord = string.Empty;

		// ウィンドウを表示してアクティブにする
		App.WindowService.ShowMainWindow();

		// ウィンドウを中央に移動する
		App.WindowService.CenterMainWindow();
	}

	public MainWindowViewModel()
	{
		_currentSearchEngine = SearchEngineManager.EngineList[0];
		SearchCommand = Command.Factory.Create(() =>
		{
			DoSearch();
			return default;
		});
	}
}
