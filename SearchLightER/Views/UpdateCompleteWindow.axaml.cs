using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class UpdateCompleteWindow : Window
{
	public UpdateCompleteWindow()
	{
		InitializeComponent();

		DataContext = new UpdateCompleteWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			e.Cancel = true;
		};
	}
}
