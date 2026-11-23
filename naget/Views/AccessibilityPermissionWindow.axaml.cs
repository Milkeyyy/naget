using Avalonia.Controls;
using naget.ViewModels;

namespace naget.Views;

public partial class AccessibilityPermissionWindow : Window
{
	private bool deactivated;

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

		Deactivated += (_, _) => deactivated = true;

		// システム設定から戻ったときに権限の状態を確認する
		Activated += (_, _) =>
		{
			if (!deactivated) return;
			if (DataContext is AccessibilityPermissionWindowViewModel vm) vm.EnableLiveCheck();
		};
	}
}
