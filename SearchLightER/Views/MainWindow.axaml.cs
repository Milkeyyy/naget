using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		DataContext = new MainWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			e.Cancel = true;
		};

		// ウィンドウが開かれた時
		Opened += (s, e) =>
		{
			// 検索テキストにフォーカスする
			this.FindControl<TextBox>("searchtextbox").Focus();
		};

		// テキスト入力のフォーカスが失われたらウィンドウを隠す
		LostFocus += (s, e) =>
		{
			Hide();
		};
	}
}
