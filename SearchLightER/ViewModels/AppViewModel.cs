using ReactiveUI;
using System;
using System.Reactive;

namespace SearchLight.ViewModels;

public class AppViewModel : ViewModelBase
{
	public ReactiveCommand<Unit, Unit> OpenInAppBrowserWindowCommand { get; }
	public ReactiveCommand<Unit, Unit> OpenSettingsWindowCommand { get; }
	public ReactiveCommand<Unit, Unit> ExitAppCommand { get; }

	public AppViewModel()
	{
		OpenInAppBrowserWindowCommand = ReactiveCommand.Create(() => {
			(App.BrowserWindow.DataContext as BrowserWindowViewModel).CurrentAddress = "https://www.google.com/";
			App.BrowserWindow.Show();
		});
		OpenSettingsWindowCommand = ReactiveCommand.Create(() => { App.SettingsWindow.Show(); });
		ExitAppCommand = ReactiveCommand.Create(() => { Environment.Exit(0); });
	}
}
