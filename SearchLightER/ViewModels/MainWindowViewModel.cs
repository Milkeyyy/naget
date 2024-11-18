using ReactiveUI;
using SearchLight.Models;
using System.Reactive;

namespace SearchLight.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public string ProductNameText { get; } = App.ProductName;
	public string ProductVersionText { get; } = App.ProductVersion;

	// 検索ワード
	private string _searchWord = string.Empty;
	public string SearchWord
	{
		get { return _searchWord; }
		set { this.RaiseAndSetIfChanged(ref _searchWord, value); }
	}

	// 選択中の検索エンジン
	private SearchEngineClass _currentSearchEngine;
	private string _currentSearchEngineId;
	public string CurrentSearchEngineId // ID
	{
		get { return _currentSearchEngine.ID; }
		set { this.RaiseAndSetIfChanged(ref _currentSearchEngineId, value); }
	}
	private string _currentSearchEngineName;
	public string CurrentSearchEngineName // 名前
	{
		get { return _currentSearchEngine.Name; }
		set { this.RaiseAndSetIfChanged(ref _currentSearchEngineName, value); }
	}

	// 検索実行コマンド
	public ReactiveCommand<Unit, Unit> SearchCommand { get; }

	public MainWindowViewModel()
	{
		// 検索エンジンを読み込む
		_currentSearchEngine = SearchEngineManager.EngineList[0];

		SearchCommand = ReactiveCommand.Create(() =>
		{
			// ブラウザーを表示
			App.BrowserWindow.Show();
			// ブラウザーで検索結果を開く
			(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = "https://www.google.com/search?q=" + SearchWord;
			// 検索テキストの内容を消す
			SearchWord = string.Empty;
			// 検索画面を閉じる
			App.MainWindow.Hide();
		});
	}
}
