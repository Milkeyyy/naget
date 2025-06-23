using Epoxy;
using naget.Models.Config;

namespace naget.ViewModels;

[ViewModel]
public class AppViewModel
{
	public Command OpenInAppBrowserWindowCommand { get; }
	public Command OpenSettingsWindowCommand { get; }
	public Command ExitAppCommand { get; }

	public AppViewModel()
	{
		OpenInAppBrowserWindowCommand = Command.Factory.Create(() =>
		{
			(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = ConfigManager.Config.BrowserWindow.StartPage;
			App.BrowserWindow.Show();
			return default;
		});
		OpenSettingsWindowCommand = Command.Factory.Create(() =>
		{
			App.SettingsWindow.Show();
			return default;
		});
		ExitAppCommand = Command.Factory.Create(() =>
		{
			App.Exit();
			return default;
		});
	}
}
