using Avalonia.Controls;
using Avalonia.Input;
using naget.ViewModels;

namespace naget.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		DataContext = new MainWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (_, e) =>
		{
			Hide();
			e.Cancel = true;
		};

		// ウィンドウが開かれた時
		Opened += (s, e) =>
		{
			// 検索テキストにフォーカスする
			this.FindControl<TextBox>("searchtextbox")?.Focus();
		};

		// テキスト入力のフォーカスが失われたらウィンドウを隠す
		LostFocus += (s, e) =>
		{
			Hide();
		};

		KeyDown += (s, e) =>
		{
			if (e.Key != Key.Escape) return;
			e.Handled = true;
			Hide();
		};
	}
}
