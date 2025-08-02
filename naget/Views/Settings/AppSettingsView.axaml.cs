using Avalonia.Controls;
using naget.ViewModels.Settings;

namespace naget.Views.Settings;

public partial class AppSettingsView : UserControl
{
	public AppSettingsView()
	{
		InitializeComponent();

		DataContext = new AppSettingsViewModel();
	}
}