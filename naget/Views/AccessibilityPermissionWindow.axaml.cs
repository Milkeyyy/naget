using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class AccessibilityPermissionWindow : Window
{
	public AccessibilityPermissionWindow()
	{
		InitializeComponent();

		DataContext = new AccessibilityPermissionWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (_, e) =>
		{
			Hide();
			e.Cancel = true;
		};
	}
}
