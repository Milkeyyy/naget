using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace naget.Views.Dialog;

public enum DialogResult
{
	None,
	Primary,
	Secondary,
	Close
}

public partial class DialogWindow : Window
{
	public static readonly StyledProperty<string?> DialogTitleProperty =
		AvaloniaProperty.Register<DialogWindow, string?>(nameof(DialogTitle));

	public static readonly StyledProperty<object?> DialogContentProperty =
		AvaloniaProperty.Register<DialogWindow, object?>(nameof(DialogContent));

	public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
		AvaloniaProperty.Register<DialogWindow, string?>(nameof(PrimaryButtonText));

	public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
		AvaloniaProperty.Register<DialogWindow, string?>(nameof(SecondaryButtonText));

	public static readonly StyledProperty<string?> CloseButtonTextProperty =
		AvaloniaProperty.Register<DialogWindow, string?>(nameof(CloseButtonText));

	public static readonly StyledProperty<bool> IsSecondaryButtonVisibleProperty =
		AvaloniaProperty.Register<DialogWindow, bool>(nameof(IsSecondaryButtonVisible), defaultValue: false);

	public string? DialogTitle
	{
		get => GetValue(DialogTitleProperty);
		set => SetValue(DialogTitleProperty, value);
	}

	public object? DialogContent
	{
		get => GetValue(DialogContentProperty);
		set => SetValue(DialogContentProperty, value);
	}

	public string? PrimaryButtonText
	{
		get => GetValue(PrimaryButtonTextProperty);
		set => SetValue(PrimaryButtonTextProperty, value);
	}

	public string? SecondaryButtonText
	{
		get => GetValue(SecondaryButtonTextProperty);
		set => SetValue(SecondaryButtonTextProperty, value);
	}

	public string? CloseButtonText
	{
		get => GetValue(CloseButtonTextProperty);
		set => SetValue(CloseButtonTextProperty, value);
	}

	public bool IsSecondaryButtonVisible
	{
		get => GetValue(IsSecondaryButtonVisibleProperty);
		set => SetValue(IsSecondaryButtonVisibleProperty, value);
	}

	public DialogWindow()
	{
		InitializeComponent();
	}

	public async Task<DialogResult> ShowAsync(Window owner)
	{
		return await ShowDialog<DialogResult>(owner);
	}

	private void OnPrimaryButtonClick(object? sender, RoutedEventArgs e)
	{
		Close(DialogResult.Primary);
	}

	private void OnSecondaryButtonClick(object? sender, RoutedEventArgs e)
	{
		Close(DialogResult.Secondary);
	}

	private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
	{
		Close(DialogResult.Close);
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);
	}
}
