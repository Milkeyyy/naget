using System;

namespace SearchLight.Models.SearchEngine;

public class SearchEngineClass(string name, string uri, string? id = null)
{
	/// <summary>
	/// 検索エンジン固有のID
	/// </summary>
	public string Id { get; set; } = id ?? Guid.NewGuid().ToString();
	/// <summary>
	/// 検索エンジンの名前
	/// </summary>
	public string Name { get; set; } = name;
	/// <summary>
	/// 検索時に使用されるURL
	/// </summary>
	public string Uri { get; set; } = uri;
}
