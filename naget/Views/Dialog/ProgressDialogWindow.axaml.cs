using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace naget.Views.Dialog;

public enum ProgressDialogResult
{
	None,
	Yes,
	No,
	Cancel,
	Close
}

public enum ProgressState
{
	Normal,
	Error
}

public class ProgressDialogButton : INotifyPropertyChanged
{
	private string _text = string.Empty;
	private bool _isEnabled = true;
	private ProgressDialogResult _result;
	private ProgressDialogWindow? _owner;

	public string Text
	{
		get => _text;
		set { _text = value; OnPropertyChanged(); }
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set { _isEnabled = value; OnPropertyChanged(); }
	}

	public ProgressDialogResult Result
	{
		get => _result;
		set => _result = value;
	}

	public ICommand ClickCommand { get; }

	public ProgressDialogButton(string text, ProgressDialogResult result)
	{
		Text = text;
		Result = result;
		ClickCommand = new RelayCommand(() => _owner?.Close(Result));
	}

	internal void SetOwner(ProgressDialogWindow owner) => _owner = owner;

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal class RelayCommand : ICommand
{
	private readonly Action _execute;
	public RelayCommand(Action execute) => _execute = execute;
	public event EventHandler? CanExecuteChanged;
	public bool CanExecute(object? parameter) => true;
	public void Execute(object? parameter) => _execute();
}

public partial class ProgressDialogWindow : Window
{
	public static readonly StyledProperty<string?> DialogTitleProperty =
		AvaloniaProperty.Register<ProgressDialogWindow, string?>(nameof(DialogTitle));

	public static readonly StyledProperty<string?> SubHeaderProperty =
		AvaloniaProperty.Register<ProgressDialogWindow, string?>(nameof(SubHeader));

	public static readonly StyledProperty<object?> DialogContentProperty =
		AvaloniaProperty.Register<ProgressDialogWindow, object?>(nameof(DialogContent));

	public static readonly StyledProperty<bool> ShowProgressBarProperty =
		AvaloniaProperty.Register<ProgressDialogWindow, bool>(nameof(ShowProgressBar), defaultValue: true);

	public static readonly StyledProperty<int> ProgressValueProperty =
		AvaloniaProperty.Register<ProgressDialogWindow, int>(nameof(ProgressValue), defaultValue: 0);

	public string? DialogTitle
	{
		get => GetValue(DialogTitleProperty);
		set => SetValue(DialogTitleProperty, value);
	}

	public string? SubHeader
	{
		get => GetValue(SubHeaderProperty);
		set => SetValue(SubHeaderProperty, value);
	}

	public object? DialogContent
	{
		get => GetValue(DialogContentProperty);
		set => SetValue(DialogContentProperty, value);
	}

	public bool ShowProgressBar
	{
		get => GetValue(ShowProgressBarProperty);
		set => SetValue(ShowProgressBarProperty, value);
	}

	public int ProgressValue
	{
		get => GetValue(ProgressValueProperty);
		set => SetValue(ProgressValueProperty, value);
	}

	public ObservableCollection<ProgressDialogButton> Buttons { get; } = [];

	private ProgressBar? _progressBar;

	public ProgressDialogWindow()
	{
		InitializeComponent();
		Buttons.CollectionChanged += (s, e) =>
		{
			if (e.NewItems != null)
			{
				foreach (ProgressDialogButton btn in e.NewItems)
				{
					btn.SetOwner(this);
				}
			}
		};
	}

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);
		_progressBar = this.FindControl<ProgressBar>("ProgressBarControl");
	}

	public void SetProgressBarState(int value, ProgressState state)
	{
		ProgressValue = value;
		if (_progressBar != null)
		{
			_progressBar.Foreground = state == ProgressState.Error
				? Brushes.Red
				: null; // Reset to default theme brush
		}
	}

	public async Task<ProgressDialogResult> ShowAsync(Window? owner = null)
	{
		if (owner != null)
		{
			return await ShowDialog<ProgressDialogResult>(owner);
		}
		Show();
		return ProgressDialogResult.None;
	}
}
