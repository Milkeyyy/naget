using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace naget.Models.SearchEngine;

public class SearchEngineList
{
	public string CreatedAt { get; set; }
	public List<SearchEngineClass> List { get; set; }

	public SearchEngineList()
	{
		CreatedAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
		List = SearchEngineManager.DefaultEngineList.ToList();
	}

	// JSONデシリアライズ用コンストラクター
	[JsonConstructor]
	public SearchEngineList(string CreatedAt, List<SearchEngineClass> List)
	{
		this.CreatedAt = CreatedAt;
		this.List = List;
	}
}
