using Avalonia;
using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace SearchLight.Views;

public static class SuperDialog
{
	private static readonly TaskDialog dialog = new();

	public static async Task Info(Visual root, string title, string content)
	{
		dialog.XamlRoot = root;
		dialog.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
		dialog.FontSize = 16;
		dialog.FontWeight = Avalonia.Media.FontWeight.SemiBold;
		dialog.Title = title;
		dialog.Content = content;
		dialog.Buttons = [TaskDialogButton.OKButton];
		await dialog.ShowAsync();
	}

}
