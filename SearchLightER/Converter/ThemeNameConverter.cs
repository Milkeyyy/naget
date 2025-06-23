using Avalonia.Data.Converters;
using naget.Assets.Locales;
using naget.Models.Config;
using System;
using System.Diagnostics;
using System.Globalization;

namespace naget.Converter;

public class ThemeNameConverter : IValueConverter
{
	public static readonly ThemeNameConverter Instance = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		Debug.WriteLine($"Convert Theme Name: {value}");
		if (value is string themeName)
		{
			return Resources.ResourceManager.GetString("Settings.Design.Theme.Name." + themeName, new CultureInfo(ConfigManager.Config.Language)) ?? themeName;
		}
		return value;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
