using Avalonia.Controls;
using Epoxy;
using naget.Assets.Locales;
using naget.Views.Settings;
using System.Threading.Tasks;

namespace naget.Views.Dialog;

public static class SuperDialog
{
	public static async Task Info(Window root, string title, string content)
	{
		var dialog = new DialogWindow
		{
			DialogTitle = title,
			DialogContent = content,
			CloseButtonText = "OK",
		};
		await dialog.ShowAsync(root);
	}

	public static async Task TaskInfo(string title, string content)
	{
		var dialog = new ProgressDialogWindow
		{
			DialogTitle = title,
			DialogContent = content,
			ShowProgressBar = false,
		};
		dialog.Buttons.Add(new ProgressDialogButton("OK", ProgressDialogResult.Close));
		var owner = App.WindowService.GetSettingsWindow();
		if (owner != null)
		{
			await dialog.ShowAsync(owner);
		}
		else
		{
			dialog.Show();
		}
	}

	public static async Task<string?> Input(string title, string inputTitle)
	{
		var dialog = new DialogWindow
		{
			// 作成画面
			DialogContent = new InputDialogContent(),

			// タイトル
			DialogTitle = title,

			// ボタンのテキスト
			PrimaryButtonText = Resources.Strings_Ok,
			CloseButtonText = Resources.Strings_Cancel,
		};

		InputDialogViewModel vm = (InputDialogViewModel)(dialog.DialogContent as InputDialogContent).DataContext;

		// ダイアログのデータコンテキストに現在の検索エンジンの情報を設定する
		vm.InputTitle = inputTitle;

		// ダイアログを表示する
		DialogResult result = await dialog.ShowAsync(App.SettingsWindow);

		// キャンセルボタンが押された場合は null を返す
		if (result != DialogResult.Primary) return null;

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
