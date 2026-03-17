using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class BrowserWindow : Window
{
	public BrowserWindow()
	{
		InitializeComponent();

		var webview = this.FindControl<NativeWebView>("webview");
		DataContext = new BrowserWindowViewModel(webview);

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			e.Cancel = true;
		};
	}
}
