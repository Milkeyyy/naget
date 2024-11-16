using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;

namespace SearchLight.ViewModels
{
	public class SettingsWindowViewModel : ViewModelBase
	{
		public string Greeting { get; } = "Welcome to Avalonia!";

		public ICommand ExitCommand { get; }

		public SettingsWindowViewModel(Action Command)
		{
			ExitCommand = ReactiveCommand.Create(Command);
		}
	}
}
