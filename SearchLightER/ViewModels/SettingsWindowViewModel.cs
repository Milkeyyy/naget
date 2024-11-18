using ReactiveUI;
using System;
using System.Reactive;

namespace SearchLight.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
	public ReactiveCommand<Unit, Unit> ExitCommand { get; }
	public SettingsWindowViewModel()
	{
		ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
	}
}
