using Avalonia.Controls;
using naget.ViewModels.Settings;

namespace naget.Views.Settings;

public partial class SystemView : UserControl
{
	public SystemView()
	{
		InitializeComponent();

		DataContext = new SystemViewModel();
	}
}