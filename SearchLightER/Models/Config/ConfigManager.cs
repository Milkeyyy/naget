using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using SearchLight.Models.Config.HotKey;
using SearchLight.Models.Config.HotKey.Action;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SearchLight.Models.Config;

public static class ConfigManager
{
	private static readonly string FilePath = Path.Join(App.ConfigFolder, "Config.json");
	//yaml private static readonly string FilePath = Path.Join(App.ConfigFolder, "Config.yml");

	private static readonly ConfigurationBuilder _builder = new();
	private static IConfigurationRoot? _config;

	private static ConfigBaseClass _configBase = new();
	/// <summary>
	/// コンフィグクラス
	/// </summary>
	public static ConfigClass Config { get { return _configBase.Config; } }

	/// <summary>
	/// ホットキーマネージャー
	/// </summary>
	public static HotKey.HotKeyManager HotKeyManager { get; } = new();

	private static readonly JsonSerializerOptions jsOptions = new()/* { IgnoreReadOnlyFields = true }*/;

	/// <summary>
	/// コンフィグを新規作成する
	/// </summary>
	public static void Create()
	{
		_configBase = new()
		{
			// 初期値が設定された ConfigClass を作成
			Config = new ConfigClass()
		};
	}

	/// <summary>
	/// コンフィグをファイルへ保存する
	/// </summary>
	public static void Save()
	{
		// ホットキーを読み込む
		//Config.HotKeys = [.. HotKeyManager.List];
		Config.HotKeys = HotKeyManager.Groups;

		Debug.WriteLine("Saving HotKeyGroup");
		foreach (var group in Config.HotKeys)
		{
			Debug.WriteLine(group.Id);
			Debug.WriteLine("- " + group.Name);
			Debug.WriteLine("- " + group.Action);
			Debug.WriteLine("- " + group);
		}

		// ファイルへ保存
		string data = JsonSerializer.Serialize(_configBase, jsOptions);
		//yaml string data = YamlSerializer.SerializeToString(_configBase);
		File.WriteAllText(FilePath, data);
	}

	/// <summary>
	/// コンフィグをファイルから読み込む
	/// </summary>
	public static void Load()
	{
		// ファイルが存在する場合はそのファイルから読み込む
		if (File.Exists(FilePath))
		{
			Debug.WriteLine("Loading config from file");
			//_config = _builder
			//	.AddJsonFile(FilePath, false)
			//	.Build();

			// ファイルから読み込んだデータをデシリアライズ (デシリアライズに失敗した場合は新規作成)
			_configBase = JsonSerializer.Deserialize<ConfigBaseClass>(File.ReadAllText(FilePath)) ?? new ConfigBaseClass();
			//yaml _configBase = YamlSerializer.Deserialize<ConfigBaseClass>(File.ReadAllBytes(FilePath));

			if (_configBase.Config == null)
			{
				Debug.WriteLine("- Config is null, Creating new config");
				Create(); // null の場合は新規作成する
				Save();
			}
		}
		// 存在しない場合は新規作成する
		else
		{
			Debug.WriteLine("Creating new config");
			Create();
			Save();
		}

		// コンフィグからホットキーマネージャーへプリセット一覧を読み込む
		if (Config.HotKeys != null)
		{
			foreach (var group in Config.HotKeys)
			{
				// キーからホットキーアクションを取得
				group.Action = HotKeyActionList.GetActionById(group.Action.Id);
			}
			HotKeyManager.LoadGroups(Config.HotKeys);
		}
	}
}
