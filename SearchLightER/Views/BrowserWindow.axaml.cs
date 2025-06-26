using Avalonia.Controls;
using naget.ViewModels;
using WebViewControl;

namespace naget.Views;

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
			e.Cancel = true;
		};
	}
}