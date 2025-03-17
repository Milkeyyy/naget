using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace SearchLight.Models.SearchEngine;

public static class SearchEngineManager
{
	private static readonly string FilePath = Path.Join(App.ConfigFolder, "SearchEngine.json");

	/// <summary>
	/// デフォルトの検索エンジン一覧
	/// </summary>
	private static readonly List<SearchEngineClass> _defaultEngineList = [
		new SearchEngineClass("Bing", "https://www.bing.com/search?form=&q={0}", "Bing"),
		new SearchEngineClass("DuckDuckGo", "https://duckduckgo.com/?q={0}", "DuckDuckGo"),
		new SearchEngineClass("Google", "https://www.google.com/search?q={0}", "Google")
	];
	/// <summary>
	/// デフォルトの検索エンジン一覧
	/// </summary>
	public static ReadOnlyCollection<SearchEngineClass> DefaultEngineList { get; } = new(_defaultEngineList);

	/// <summary>
	/// 検索エンジン (<c>SearchEngineClass</c>) のリスト (コレクション)
	/// </summary>
	private static SearchEngineList _engineList = new();
	/// <summary>
	/// 検索エンジン (<c>SearchEngineClass</c>) のリスト
	/// </summary>
	public static ReadOnlyCollection<SearchEngineClass> EngineList { get { return _engineList.List.AsReadOnly(); } }

	public static SearchEngineClass GetDefaultEngine()
	{
		return _defaultEngineList[0];
	}

	/// <summary>
	/// 検索エンジンのリストを新規作成する
	/// </summary>
	public static void Create()
	{
		//_engineCollection.List = [.. _defaultEngine];
		_engineList.List = new(_defaultEngineList);
	}

	/// <summary>
	/// 検索エンジンのリストをファイルへ保存する
	/// </summary>
	public static void Save()
	{
		string data = JsonSerializer.Serialize(_engineList);
		File.WriteAllText(FilePath, data);
	}

	/// <summary>
	/// 検索エンジンのリストをファイルから読み込む
	/// </summary>
	public static void Load()
	{
		// ファイルが存在する場合はそのファイルから読み込む
		if (File.Exists(FilePath))
		{
			// デシリアライズ
			SearchEngineList data = JsonSerializer.Deserialize<SearchEngineList>(File.ReadAllText(FilePath)) ?? new SearchEngineList();
			
			if (data != null)
			{
				_engineList = data;
			}
			else
			{
				Create(); // null の場合は新規作成する
				Save();
			}
		}
		// 存在しない場合は新規作成する
		else
		{
			Create();
			Save();
		}
	}

	/// <summary>
	/// 指定されたIDの検索エンジンを取得する
	/// </summary>
	/// <param name="id">対象となる検索エンジンのID</param>
	/// <returns>指定されたIDに該当する検索エンジン 見つからない場合は <p>null</p></returns>
	public static SearchEngineClass? Get(string? id = null)
	{
		if (id == null) return _engineList.List[0];
		return _engineList.List.Find(x => x.Id == id);
	}

	/// <summary>
	/// 新しい検索エンジンを作成する
	/// </summary>
	/// <param name="name">名前</param>
	/// <param name="uri">URL 検索ワードは {0}</param>
	public static void Create(string name, string uri)
	{
		_engineList.List.Add(new SearchEngineClass(name, uri));
	}

	/// <summary>
	/// 指定されたIDの検索エンジンを削除する
	/// </summary>
	/// <param name="id">対象となる検索エンジンのID</param>
	/// <exception cref="KeyNotFoundException"></exception>
	public static bool Delete(string id)
	{
		if (_engineList.List.Count == 1) return false;

		var result = Get(id);
		if (result != null)
		{
			_engineList.List.Remove(result);
			return true;
		}
		else
		{
			throw new KeyNotFoundException(id);
		}
	}

	/// <summary>
	/// 指定されたIDの検索エンジンの名前を変更する
	/// </summary>
	/// <param name="id">対象となる検索エンジンのID</param>
	/// <param name="newName">変更する名前</param>
	/// /// <exception cref="KeyNotFoundException"></exception>
	public static void Rename(string id, string newName)
	{
		var result = Get(id);
		if (result != null)
		{
			result.Name = newName;
		}
		else
		{
			throw new KeyNotFoundException(id);
		}
	}

	/// <summary>
	/// 指定されたIDのURLを更新する
	/// </summary>
	/// <param name="id">対象となる検索エンジンのID</param>
	/// <param name="newUri">変更するURL</param>
	/// <exception cref="KeyNotFoundException"></exception>
	public static void UpdateUri(string id, string newUri)
	{
		var result = Get(id);
		if (result != null)
		{
			result.Uri = newUri;
		}
		else
		{
			throw new KeyNotFoundException(id);
		}
	}
}
