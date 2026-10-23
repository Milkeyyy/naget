using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class LicenseWindow : Window
{
	public LicenseWindow()
	{
		InitializeComponent();

		DataContext = new LicenseWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (_, e) =>
		{
			Hide();
			e.Cancel = true;
		};
	}
}
