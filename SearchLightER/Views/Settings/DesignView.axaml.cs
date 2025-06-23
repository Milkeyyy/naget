using Avalonia.Controls;
using naget.ViewModels.Settings;

namespace naget.Views.Settings;

public partial class DesignView : UserControl
{
	public DesignView()
	{
		InitializeComponent();

		DataContext = new DesignViewModel();
	}
}