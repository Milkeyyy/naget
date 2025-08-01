﻿using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace naget.Models.Config;

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
	// 対応言語一覧
	[JsonIgnore]
	public static readonly ReadOnlyCollection<Language> LanguageList = new([
		new("en-US", "English (US)"),
		new("ja-JP", "日本語 - Japanese")
	]);

	// 対応テーマ一覧
	[JsonIgnore]
	public static readonly ReadOnlyCollection<string> ThemeList = new([
		"Default",
		"Dark",
		"Light"
	]);

	// コンフィグの各項目
	public string Language { get; set; } // 言語コード ("en-US", "ja-JP")
	public string Theme { get; set; } // テーマの名前 ("Default", "Dark", "Light")
	public List<HotKey.HotKeyGroup> HotKeys { get; set; }
	public BrowserWindowConfig BrowserWindow { get; set; }

	public ConfigClass()
	{
		// 初期値の設定
		Language = LanguageList[0].Code;
		Theme = ThemeList[0]; // システム設定
		HotKeys = [];
		BrowserWindow = new BrowserWindowConfig();
	}

	[JsonConstructor]
	public ConfigClass(string language, string theme, List<HotKey.HotKeyGroup> hotKeys, BrowserWindowConfig browserWindow)
	{
		Language = language;
		Theme = theme;
		HotKeys = hotKeys;
		BrowserWindow = browserWindow;
	}
}

public class WindowConfig
{
	public WindowState State { get; set; }
	public double Width { get; set; }
	public double Height { get; set; }

	public WindowConfig()
	{
		State = WindowState.Normal;
		Width = 1280;
		Height = 720;
	}

	[JsonConstructor]
	public WindowConfig(WindowState state, double width, double height)
	{
		State = state;
		Width = width;
		Height = height;
	}
}

public class BrowserWindowConfig : WindowConfig
{
	public string StartPage { get; set; }

	public BrowserWindowConfig()
	{
		StartPage = "https://www.google.com/";
	}

	[JsonConstructor]
	public BrowserWindowConfig(WindowState state, double width, double height, string startPage)
	{
		State = state;
		Width = width;
		Height = height;
		StartPage = startPage;
	}
}

public class Language(string code, string displayName)
{
	public string Code { get; } = code;
	public string DisplayName { get; } = displayName;
}

/*public class BrowserWindowConfig
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
}*/
