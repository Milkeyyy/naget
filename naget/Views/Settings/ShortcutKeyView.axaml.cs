using Avalonia.Controls;
using Avalonia.Interactivity;
using naget.ViewModels.Settings;

namespace naget.Views.Settings;

public partial class ShortcutKeyView : UserControl
{
	public ShortcutKeyView()
	{
		InitializeComponent();

		DataContext = new ShortcutKeyViewModel();
	}

	private void OnKeyRegisterButtonClick(object? sender, RoutedEventArgs e)
	{
		Focus();
	}
}