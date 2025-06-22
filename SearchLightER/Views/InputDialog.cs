using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace naget.Views;

public class InputDialog
{
	private readonly ContentDialog dialog;

	public InputDialog()
	{
		dialog = new ContentDialog()
		{
			Title = "",
			PrimaryButtonText = "OK",
			SecondaryButtonText = "Cancel",
			CloseButtonText = "Close"
		};
	}

	public async Task<ContentDialogResult> ShowAsync(string title, string primaryButtonText, string secondaryButtonText, string closeButtonText)
	{
		dialog.Title = title;
		dialog.PrimaryButtonText = primaryButtonText;
		dialog.SecondaryButtonText = secondaryButtonText;
		dialog.CloseButtonText = closeButtonText;
		return await dialog.ShowAsync();
	}
}
