using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using SearchLight.Models.Config.HotKey;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SearchLight.Models.Config;

public static class ConfigManager
{
	private static readonly string FilePath = Path.Join(App.ConfigFolder, "Config.json");

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

	/// <summary>
	/// コンフィグを新規作成する
	/// </summary>
	public static void Create()
	{
		_configBase = new()
		{
			// 初期値が設定された ConfigClass を作成
			Config = ConfigClass.Create()
		};
	}

	/// <summary>
	/// コンフィグをファイルへ保存する
	/// </summary>
	public static void Save()
	{
		// ホットキーを読み込む
		//Config.HotKeys = [.. HotKeyManager.List];
		Config.HotKeys = new(HotKeyManager.List);
		// ファイルへ保存
		string data = JsonSerializer.Serialize(_configBase);
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
			_config = _builder
				.AddJsonFile(FilePath)
				.Build();

			_configBase = JsonSerializer.Deserialize<ConfigBaseClass>(File.ReadAllText(FilePath));

			if (_configBase != null)
			{
				if (_configBase.Config == null)
				{
					Debug.WriteLine("- Config is null, Creating new config");
					Create(); // null の場合は新規作成する
					Save();
				}
			}
			else
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
		HotKeyManager.LoadGroups(Config.HotKeys);
	}
}
