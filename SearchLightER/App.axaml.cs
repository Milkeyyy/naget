using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SearchLight.ViewModels;
using SearchLight.Views;

namespace SearchLight
{
	public partial class App : Application
	{
		public static Window SettingsWindow;

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			Assets.Locales.Resources.Culture = new CultureInfo("ja-JP");
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new MainWindow
				{
					DataContext = new MainWindowViewModel(),
				};
			}

			SettingsWindow = new SettingsWindow();

			base.OnFrameworkInitializationCompleted();
		}
	}
}