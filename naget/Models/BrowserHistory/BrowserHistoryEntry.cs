using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace naget.Models.BrowserHistory;

public class BrowserHistoryEntry
{
	public string Url { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public DateTimeOffset VisitedAt { get; set; }

	[JsonIgnore]
	public string DisplayTitle { get { return string.IsNullOrWhiteSpace(Title) ? Url : Title; } }

	[JsonIgnore]
	public string VisitedAtText { get { return VisitedAt.LocalDateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture); } }

	public BrowserHistoryEntry()
	{
	}

	[JsonConstructor]
	public BrowserHistoryEntry(string url, string title, DateTimeOffset visitedAt)
	{
		Url = url;
		Title = title;
		VisitedAt = visitedAt;
	}
}
