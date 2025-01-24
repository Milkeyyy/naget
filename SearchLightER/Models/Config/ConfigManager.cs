using Microsoft.Extensions.Configuration;
using SearchLight.Models.SearchEngine;
using System.IO;
using System.Text.Json;

namespace SearchLight.Models.Config;

public static class ConfigManager
{
	private static readonly string FilePath = Path.Join(App.ConfigFolder, "Config.json");

	private static readonly ConfigurationBuilder _builder = new();
	private static IConfigurationRoot? _config;

	public static ConfigClass Config { get; set; } = new();

	/// <summary>
	/// コンフィグを新規作成する
	/// </summary>
	public static void Create()
	{
		Config = new();
	}

	/// <summary>
	/// コンフィグをファイルへ保存する
	/// </summary>
	public static void Save()
	{
		string data = JsonSerializer.Serialize(Config);
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
			_config = _builder
				.AddJsonFile(FilePath)
				.Build();
			ConfigClass? data = _config.Get<ConfigClass>();
			if (data != null) Config = data;
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
}
