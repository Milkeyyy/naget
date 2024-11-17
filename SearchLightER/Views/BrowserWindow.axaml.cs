using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SearchLight.ViewModels;
using WebViewControl;

namespace SearchLight;

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