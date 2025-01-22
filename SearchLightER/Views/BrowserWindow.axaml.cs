using Avalonia.Controls;
using SearchLight.ViewModels;
using WebViewControl;

namespace SearchLight.Views;

public partial class BrowserWindow : Window
{
	public BrowserWindow()
	{
		InitializeComponent();

		DataContext = new BrowserWindowViewModel(this.FindControl<WebView>("webview"));

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			(DataContext as BrowserWindowViewModel).CurrentAddress = "about:blank";
			e.Cancel = true;
		};
	}
}