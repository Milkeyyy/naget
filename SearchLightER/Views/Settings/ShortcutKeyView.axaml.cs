using Avalonia.Controls;
using naget.ViewModels.Settings;

namespace naget.Views.Settings;

public partial class ShortcutKeyView : UserControl
{
	public ShortcutKeyView()
	{
		InitializeComponent();

		DataContext = new ShortcutKeyViewModel();
	}
}