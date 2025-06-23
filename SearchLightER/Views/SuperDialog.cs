using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace naget.Views;

public static class SuperDialog
{
	public static async Task Info(Window root, string title, string content)
	{
		ContentDialog dialog = new()
		{
			//HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			//VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
			//FontSize = 16,
			//FontWeight = Avalonia.Media.FontWeight.SemiBold,
			Title = title,
			Content = content,
			CloseButtonText = "OK",
			IsPrimaryButtonEnabled = false,
			IsSecondaryButtonEnabled = false
		};
		await dialog.ShowAsync(root);
	}

}
