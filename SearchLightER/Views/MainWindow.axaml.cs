using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SearchLight.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public void OpenSettingsWindow(object sender, RoutedEventArgs args)
		{
			App.SettingsWindow.Show();
		}
	}
}