using Avalonia.Controls;
using naget.Views.Dialog;

namespace naget.Views.Settings;

public partial class InputDialogContent : UserControl
{
	public InputDialogContent()
	{
		InitializeComponent();

		DataContext = new InputDialogViewModel();
	}
}
