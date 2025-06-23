using Avalonia.Controls;
using Epoxy;
using naget.Models.Config;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class DesignViewModel
{
	public Well<UserControl> ViewWell { get; } = Well.Factory.Create<UserControl>();

	public static ReadOnlyCollection<string> ThemeList => ConfigClass.ThemeList;
	public string SelectedTheme { get; set; }

	private bool ViewIsLoaded;

	public DesignViewModel()
	{
		// ビューがロードされた時の処理
		ViewWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("DesignView Loaded");
			// コンフィグのテーマ設定から該当するテーマを取得して選択中テーマに設定する
			SelectedTheme = ThemeList.FirstOrDefault(t => t == ConfigManager.Config.Theme) ?? ThemeList[0];
			ViewIsLoaded = true;
			return default;
		});
	}

	[PropertyChanged(nameof(SelectedTheme))]
	private ValueTask SelectedThemeChangedAsync(string value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		Debug.WriteLine($"Selected Theme Changed: {value}");
		// アプリ全体のテーマを変更
		App.ChangeTheme(value);
		// コンフィグのテーマ設定を更新
		ConfigManager.Config.Theme = value;
		return default;
	}
}
