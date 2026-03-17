using naget.Views.Dialog;
using System.Threading.Tasks;

namespace naget.Views;

public class InputDialog
{
	private readonly DialogWindow dialog;

	public InputDialog()
	{
		dialog = new DialogWindow()
		{
			DialogTitle = "",
			PrimaryButtonText = "OK",
			SecondaryButtonText = "Cancel",
			CloseButtonText = "Close"
		};
	}

	public async Task<DialogResult> ShowAsync(string title, string primaryButtonText, string secondaryButtonText, string closeButtonText)
	{
		dialog.DialogTitle = title;
		dialog.PrimaryButtonText = primaryButtonText;
		dialog.SecondaryButtonText = secondaryButtonText;
		dialog.IsSecondaryButtonVisible = true;
		dialog.CloseButtonText = closeButtonText;
		return await dialog.ShowAsync(App.SettingsWindow);
	}
}
