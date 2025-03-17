using System;
using System.Collections.Generic;
using System.Globalization;

namespace SearchLight.Models.Config;

public class ConfigBaseClass
{
	public string CreatedAt { get; set; } = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
	public ConfigClass Config { get; set; } = new();
}


public class ConfigClass
{
	public string Language { get; set; }
	public List<HotKey.HotKeyGroup> HotKeys { get; set; }

	public ConfigClass()
	{
		Language = CultureInfo.CurrentCulture.Name;
		HotKeys = [];
	}
}
