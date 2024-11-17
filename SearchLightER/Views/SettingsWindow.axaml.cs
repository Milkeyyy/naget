using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SearchLight.ViewModels;

namespace SearchLight.Views
{
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();

			DataContext = new SettingsWindowViewModel();

			// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
			Closing += (s, e) =>
			{
				((Window)s).Hide();
				e.Cancel = true;
			};
		}
	}
}