using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class BrowserWindow : Window
{
	public BrowserWindow()
	{
		InitializeComponent();

		var webview = this.FindControl<NativeWebView>("webview");
		var vm = new BrowserWindowViewModel(webview);
		DataContext = vm;

		var addressBox = this.FindControl<TextBox>("AddressBox");
		addressBox!.GotFocus += (s, e) => vm.AddressBoxFocused = true;
		addressBox!.LostFocus += (s, e) => vm.AddressBoxFocused = false;

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			e.Cancel = true;
		};
	}
}
