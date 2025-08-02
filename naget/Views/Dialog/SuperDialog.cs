using Avalonia.Controls;
using Epoxy;
using FluentAvalonia.UI.Controls;
using naget.Assets.Locales;
using naget.Views.Settings;
using System.Threading.Tasks;

namespace naget.Views.Dialog;

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

	public static async Task TaskInfo(string title, string content)
	{
		var dialog = new TaskDialog
		{
			Title = title,
			Content = content,
			Buttons = {
			new TaskDialogButton { Text = "OK" }
		}
		};
		await dialog.ShowAsync();
	}

	public static async Task<string?> Input(string title, string inputTitle)
	{
		var dialog = new ContentDialog
		{
			// 作成画面
			Content = new InputDialogContent(),

			// タイトル
			Title = title,

			// ボタンのテキスト
			IsSecondaryButtonEnabled = false, // 第二ボタンを無効化
			PrimaryButtonText = Resources.Strings_Ok,
			CloseButtonText = Resources.Strings_Cancel,
		};

		InputDialogViewModel vm = (InputDialogViewModel)(dialog.Content as InputDialogContent).DataContext;

		// ダイアログのデータコンテキストに現在の検索エンジンの情報を設定する
		vm.InputTitle = inputTitle;

		// ダイアログを表示する
		ContentDialogResult result = await dialog.ShowAsync();

		// キャンセルボタンが押された場合は null を返す
		if (result == ContentDialogResult.None) return null;

		// 入力内容を返す
		return vm.InputValue;
	}
}

[ViewModel]
public class InputDialogViewModel
{
	public string InputTitle { get; set; } = string.Empty;
	public string InputValue { get; set; } = string.Empty;
}
