using Avalonia.Controls;

namespace naget.Views.Settings;

public partial class InAppBrowserView : UserControl
{
	public InAppBrowserView()
	{
		InitializeComponent();

		DataContext = new ViewModels.Settings.InAppBrowserViewModel();
	}
}