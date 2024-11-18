using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;

namespace SearchLight.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
	public ReactiveCommand<Unit, Unit> ExitCommand { get; }
	public SettingsWindowViewModel()
	{
		ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
	}
}
