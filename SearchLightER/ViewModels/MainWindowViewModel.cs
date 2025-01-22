using Epoxy;
using SearchLight.Models;

namespace SearchLight.ViewModels;

[ViewModel]
public class MainWindowViewModel
{
	public string ProductNameText { get; } = App.ProductName;
	public string ProductVersionText { get; } = App.ProductVersion;
	public string SearchWord { get; set; } = string.Empty;

	// 選択中の検索エンジン
	private SearchEngineClass _currentSearchEngine;
	private string _currentSearchEngineId;
	public string CurrentSearchEngineId // ID
	{
		get { return _currentSearchEngine.ID; }
		set { _currentSearchEngineId = value; }
	}
	private string _currentSearchEngineName;
	public string CurrentSearchEngineName // 名前
	{
		get { return _currentSearchEngine.Name; }
		set { _currentSearchEngineName = value; }
	}

	// 検索実行コマンド
	public Command SearchCommand { get; }

	public MainWindowViewModel()
	{
		// 検索エンジンを読み込む
		_currentSearchEngine = SearchEngineManager.EngineList[0];

		SearchCommand = Command.Factory.Create(() =>
		{
			// ブラウザーを表示
			App.BrowserWindow.Show();
			// ブラウザーで検索結果を開く
			(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = "https://www.google.com/search?q=" + SearchWord;
			// 検索テキストの内容を消す
			SearchWord = string.Empty;
			// 検索画面を閉じる
			App.MainWindow.Hide();
			return default;
		});
	}
}
