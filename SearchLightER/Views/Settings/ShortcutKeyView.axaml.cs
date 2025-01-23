using Avalonia.Controls;
using SearchLight.ViewModels.Settings;

namespace SearchLight.Views.Settings;

public partial class ShortcutKeyView : UserControl
{
	public ShortcutKeyView()
	{
		InitializeComponent();

		DataContext = new ShortcutKeyViewModel();
	}
}