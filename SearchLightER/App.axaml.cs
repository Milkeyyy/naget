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
using System.Reflection;

namespace SearchLight
{
	public partial class App : Application
	{
		private static string _name = "";
		public static string ProductName => _name;
		private static string _version = "";
		public static string ProductVersion => _version;

		public static Window SettingsWindow;

		public override void Initialize()
		{
			var a = Assembly.GetExecutingAssembly().GetName();
			_name = a.Name;
			_version = a.Version.ToString();
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			Assets.Locales.Resources.Culture = new CultureInfo("ja-JP");
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
				
				// �z�b�g�L�[
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
					() => desktop.Shutdown() // �ݒ��ʂ̏I���{�^���̃C�x���g�� Shutdown() ���o�C���h����
				);

				desktop.MainWindow = new MainWindow
				{
					DataContext = MainWindowViewModel
				};

				SettingsWindow = new SettingsWindow
				{
					DataContext = SettingsWindowModel
				};

				// �z�b�g�L�[�������C�x���g
				hotKeyManager.HotKeyPressed
					.ObserveOn(Avalonia.ReactiveUI.AvaloniaScheduler.Instance)
					.Subscribe(_ => SettingsWindow.Show()); // �ݒ��ʂ��J��
					//.Subscribe(hotKey => MainWindowViewModel.Text += $"HotKey: Id={hotKey.Id}, Key={hotKey.Key}, Modifiers={hotKey.Modifiers}{Environment.NewLine}");
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}