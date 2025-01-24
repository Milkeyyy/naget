using System;
using System.Collections.Generic;

namespace SearchLight.Models.SearchEngine;

public class SearchEngineList
{
	public SearchEngineList()
	{
		CreatedAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
		List = [];
	}

	public string CreatedAt { get; set; }

	public List<SearchEngineClass> List { get; set; }
}
