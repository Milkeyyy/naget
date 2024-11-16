using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GlobalHotKeys.Native.Types;
using SearchLight.ViewModels;
using SearchLight.Views;
using System;
using System.Globalization;
using System.Reactive.Linq;

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
				desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
				
				// ホットキー
				var hotKeyManager = new GlobalHotKeys.HotKeyManager();
				var hotKeySubscription = hotKeyManager.Register(
					VirtualKeyCode.KEY_D,
					Modifiers.Control | Modifiers.Alt // Ctrl + Alt
				);

				desktop.Exit += (sender, args) =>
				{
					hotKeySubscription.Dispose();
					hotKeyManager.Dispose();
				};

				var MainWindowViewModel = new MainWindowViewModel();
				var SettingsWindowModel = new SettingsWindowViewModel(
					() => desktop.Shutdown() // 設定画面の終了ボタンのイベントに Shutdown() をバインドする
				);

				desktop.MainWindow = new MainWindow
				{
					DataContext = MainWindowViewModel
				};

				SettingsWindow = new SettingsWindow
				{
					DataContext = SettingsWindowModel
				};

				hotKeyManager.HotKeyPressed
					.ObserveOn(Avalonia.ReactiveUI.AvaloniaScheduler.Instance)
					.Subscribe(_ => SettingsWindow.Show());
					//.Subscribe(hotKey => MainWindowViewModel.Text += $"HotKey: Id={hotKey.Id}, Key={hotKey.Key}, Modifiers={hotKey.Modifiers}{Environment.NewLine}");
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}