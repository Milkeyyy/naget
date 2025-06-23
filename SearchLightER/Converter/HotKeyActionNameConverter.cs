using Avalonia.Data.Converters;
using naget.Assets.Locales;
using naget.Models.Config;
using System;
using System.Diagnostics;
using System.Globalization;

namespace naget.Converter;

public class HotKeyActionNameConverter : IValueConverter
{
	public static readonly HotKeyActionNameConverter Instance = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		Debug.WriteLine($"Convert HotKey Action Name: {value}");
		if (value is string actionName)
		{
			return Resources.ResourceManager.GetString("Settings.ShortcutKey.Action." + actionName, new CultureInfo(ConfigManager.Config.Language)) ?? actionName;
		}
		return value;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
