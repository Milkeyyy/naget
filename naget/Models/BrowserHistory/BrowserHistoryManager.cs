using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace naget.Models.BrowserHistory;

public static class BrowserHistoryManager
{
	private const int MaxEntries = 1000;

	private static readonly string FilePath = Path.Join(App.ConfigFolder, "History.json");

	private static List<BrowserHistoryEntry> _entries = [];

	public static ReadOnlyCollection<BrowserHistoryEntry> Entries { get { return _entries.AsReadOnly(); } }

	public static void Add(string url, string title)
	{
		if (string.IsNullOrWhiteSpace(url)) return;
		if (url == "about:blank") return;

		if (_entries.Count > 0 && _entries[0].Url == url)
		{
			BrowserHistoryEntry newest = _entries[0];
			if (!string.IsNullOrWhiteSpace(title)) newest.Title = title;
			newest.VisitedAt = DateTimeOffset.Now;
		}
		else
		{
			_entries.Insert(0, new BrowserHistoryEntry(url, title, DateTimeOffset.Now));
			if (_entries.Count > MaxEntries)
			{
				_entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
			}
		}

		App.Logger.Debug("Browser History Added: " + url);
		Save();
	}

	public static void Clear()
	{
		_entries.Clear();
		Save();
	}

	public static void Load()
	{
		if (!File.Exists(FilePath)) return;

		try
		{
			_entries = JsonSerializer.Deserialize<List<BrowserHistoryEntry>>(File.ReadAllText(FilePath)) ?? [];
			App.Logger.Debug($"Browser History Loaded: {_entries.Count} entries");
		}
		catch (Exception ex)
		{
			App.Logger.Warn("Failed to load browser history: " + ex.Message);
			_entries = [];
		}
	}

	private static void Save()
	{
		string data = JsonSerializer.Serialize(_entries);
		string tempPath = FilePath + ".tmp";
		File.WriteAllText(tempPath, data);
		File.Move(tempPath, FilePath, true);
	}
}
