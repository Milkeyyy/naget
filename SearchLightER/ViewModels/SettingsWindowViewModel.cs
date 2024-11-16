using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;

namespace SearchLight.ViewModels
{
	public class SettingsWindowViewModel(Action Command) : ViewModelBase
	{
		public ICommand ExitCommand { get; } = ReactiveCommand.Create(Command);
	}
}
