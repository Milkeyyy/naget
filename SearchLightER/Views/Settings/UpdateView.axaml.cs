using Avalonia.Controls;

namespace naget.Views.Settings;

public partial class UpdateView : UserControl
{
	public UpdateView()
	{
		InitializeComponent();

		DataContext = new ViewModels.Settings.UpdateViewModel();
	}
}