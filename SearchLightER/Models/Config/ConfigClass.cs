using System;
using System.Collections.Generic;
using System.Globalization;

namespace SearchLight.Models.Config;

public class ConfigClass
{
	public ConfigClass()
	{
		CreatedAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
	}

	public string CreatedAt { get; set; }

	public string Language { get; set; } = CultureInfo.CurrentCulture.Name;

	public List<HotKey.HotKeyGroup> HotKeys { get; set; } = [];
}
