using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SearchLight.Views
{
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
			Closing += (s, e) =>
			{
				((Window)s).Hide();
				e.Cancel = true;
			};
		}
	}
}