using ReactiveUI;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;

namespace SearchLight.ViewModels
{
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
		// 検索コマンド
		public ReactiveCommand<Unit, Unit> SearchCommand { get; }
		public MainWindowViewModel()
		{
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
}
