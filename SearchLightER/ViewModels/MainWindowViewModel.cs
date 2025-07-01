using Epoxy;
using naget.Helpers;
using naget.Models.SearchEngine;

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
		// ブラウザーを表示
		App.BrowserWindow.Show();
		// ブラウザーで検索結果を開く
		(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = string.Format(_currentSearchEngine.Uri, SearchWord);
		// 検索画面を閉じる
		App.MainWindow.Hide();
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

		// ウィンドウを表示
		App.MainWindow.Show();	

		// マウスカーソルがあるディスプレイの中央位置を取得
		var centerPos = HotKeyHelper.GetCenterScreen(App.MainWindow);

		// 中央位置を取得できた場合はウィンドウの位置をその中央へ移動する
		App.MainWindow.Position = centerPos ?? new(0,0);

		// ウィンドウをアクティブにする
		App.MainWindow.Activate();
		App.MainWindow.Focus();
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
