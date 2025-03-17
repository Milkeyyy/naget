using System;
using System.Collections.Generic;

namespace SearchLight.Models.SearchEngine;

public class SearchEngineList
{
	public SearchEngineList()
	{
		CreatedAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
		List = [
			new SearchEngineClass("Bing", "https://www.bing.com/search?form=&q={0}", "Bing"),
			new SearchEngineClass("DuckDuckGo", "https://duckduckgo.com/?q={0}", "DuckDuckGo"),
			new SearchEngineClass("Google", "https://www.google.com/search?q={0}", "Google")
		];
	}

	public string CreatedAt { get; set; }

	public List<SearchEngineClass> List { get; set; }
}
