using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace naget.Controls;

public class SettingsCard : TemplatedControl
{
	public static readonly StyledProperty<object?> HeaderProperty =
		AvaloniaProperty.Register<SettingsCard, object?>(nameof(Header));

	public static readonly StyledProperty<string?> DescriptionProperty =
		AvaloniaProperty.Register<SettingsCard, string?>(nameof(Description));

	public static readonly StyledProperty<string?> IconProperty =
		AvaloniaProperty.Register<SettingsCard, string?>(nameof(Icon));

	public static readonly StyledProperty<object?> FooterProperty =
		AvaloniaProperty.Register<SettingsCard, object?>(nameof(Footer));

	public static readonly StyledProperty<string?> ActionIconProperty =
		AvaloniaProperty.Register<SettingsCard, string?>(nameof(ActionIcon));

	public static readonly StyledProperty<bool> IsExpandedProperty =
		AvaloniaProperty.Register<SettingsCard, bool>(nameof(IsExpanded), defaultValue: false);

	public static readonly StyledProperty<bool> IsClickEnabledProperty =
		AvaloniaProperty.Register<SettingsCard, bool>(nameof(IsClickEnabled), defaultValue: false);

	public static readonly StyledProperty<ICommand?> CommandProperty =
		AvaloniaProperty.Register<SettingsCard, ICommand?>(nameof(Command));

	public static readonly StyledProperty<object?> BodyProperty =
		AvaloniaProperty.Register<SettingsCard, object?>(nameof(Body));

	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	public string? Description
	{
		get => GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	public string? Icon
	{
		get => GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	public object? Footer
	{
		get => GetValue(FooterProperty);
		set => SetValue(FooterProperty, value);
	}

	public string? ActionIcon
	{
		get => GetValue(ActionIconProperty);
		set => SetValue(ActionIconProperty, value);
	}

	public bool IsExpanded
	{
		get => GetValue(IsExpandedProperty);
		set => SetValue(IsExpandedProperty, value);
	}

	public bool IsClickEnabled
	{
		get => GetValue(IsClickEnabledProperty);
		set => SetValue(IsClickEnabledProperty, value);
	}

	public ICommand? Command
	{
		get => GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public object? Body
	{
		get => GetValue(BodyProperty);
		set => SetValue(BodyProperty, value);
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		var headerButton = e.NameScope.Find<Button>("PART_HeaderButton");
		if (headerButton != null)
		{
			headerButton.Click += (s, args) =>
			{
				if (IsClickEnabled && Command?.CanExecute(null) == true)
				{
					Command.Execute(null);
				}
				else if (Body != null)
				{
					IsExpanded = !IsExpanded;
				}
			};
		}
	}
}
