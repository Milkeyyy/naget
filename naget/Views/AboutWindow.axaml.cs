using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class AboutWindow : Window
{
	public AboutWindow()
	{
		InitializeComponent();

		DataContext = new AboutWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (_, e) =>
		{
			Hide();
			e.Cancel = true;
		};
	}
}
