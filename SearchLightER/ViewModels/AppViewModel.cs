using Epoxy;

namespace SearchLight.ViewModels;

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
			(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = "https://www.google.com/";
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
