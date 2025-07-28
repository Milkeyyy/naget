using Epoxy;
using naget.Models.Config;

namespace naget.ViewModels;

[ViewModel]
public class AppViewModel
{
	public Command OpenAboutCommand { get; }
	public Command OpenInAppBrowserWindowCommand { get; }
	public Command OpenSettingsWindowCommand { get; }
	public Command CheckUpdateCommand { get; }
	public Command ExitAppCommand { get; }

	public AppViewModel()
	{
		#region コンテキストメニューの各コマンド
		// About
		OpenAboutCommand = Command.Factory.Create(() =>
		{
			App.AboutWindow.Show();
			return default;
		});
		// アプリ内ブラウザー
		OpenInAppBrowserWindowCommand = Command.Factory.Create(() =>
		{
			// 既にブラウザーが開いている場合はフォーカス(アクティブに)する
			if (App.BrowserWindow.IsVisible)
			{
				// 一時的に最前面に設定して無理やり手前に持ってくる
				App.BrowserWindow.Topmost = true;
				App.BrowserWindow.Focus(); // フォーカスできないけど一応実行する
				App.BrowserWindow.Topmost = false;
				return default;
			}
			// スタートページを開いてウィンドウを表示する
			(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = ConfigManager.Config.BrowserWindow.StartPage;
			App.BrowserWindow.Show();
			return default;
		});
		// 設定画面
		OpenSettingsWindowCommand = Command.Factory.Create(() =>
		{
			App.SettingsWindow.Show();
			return default;
		});
		// アップデートチェック
		CheckUpdateCommand = Command.Factory.Create(async () =>
		{
			await App.Updater.ManualCheck();
		});
		// アプリケーション終了
		ExitAppCommand = Command.Factory.Create(() =>
		{
			App.Exit();
			return default;
		});
		#endregion
	}
}
