using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SearchLight.Models.Config;

public class ConfigBaseClass
{
	public string CreatedAt { get; set; } = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
	public ConfigClass Config { get; set; } = new();

	public ConfigBaseClass()
	{
		Config = new();
	}

	[JsonConstructor]
	public ConfigBaseClass(string CreatedAt, ConfigClass Config)
	{
		this.CreatedAt = CreatedAt;
		this.Config = Config;
	}
}


public class ConfigClass
{
	public string Language { get; set; }
	public List<HotKey.HotKeyGroup> HotKeys { get; set; }
	public BrowserWindowConfig BrowserWindow { get; set; }

	public ConfigClass()
	{
		Language = CultureInfo.CurrentCulture.Name;
		HotKeys = [];
		BrowserWindow = new BrowserWindowConfig();
	}

	[JsonConstructor]
	public ConfigClass(string Language, List<HotKey.HotKeyGroup> HotKeys, BrowserWindowConfig BrowserWindow)
	{
		this.Language = Language;
		this.HotKeys = HotKeys;
		this.BrowserWindow = BrowserWindow;
	}
}

public class BrowserWindowConfig
{
	public WindowState State { get; set; }
	public double Width { get; set; }
	public double Height { get; set; }

	public BrowserWindowConfig()
	{
		State = WindowState.Normal;
		Width = 1280;
		Height = 720;
	}

	[JsonConstructor]
	public BrowserWindowConfig(WindowState State, double Width, double Height)
	{
		this.State = State;
		this.Width = Width;
		this.Height = Height;
	}
}
