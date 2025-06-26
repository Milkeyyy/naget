using Avalonia.Controls;
using Epoxy;
using naget.Models.Config;
using System.Diagnostics;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class InAppBrowserViewModel
{
	public Well<UserControl> ViewWell { get; } = Well.Factory.Create<UserControl>();

	public string StartPageUrl { get; set; } = string.Empty;

	private bool ViewIsLoaded;

	public InAppBrowserViewModel()
	{
		// ビューがロードされた時の処理
		ViewWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("InAppBrowserView Loaded");
			// コンフィグのテーマ設定から該当するテーマを取得して選択中テーマに設定する
			StartPageUrl = ConfigManager.Config.BrowserWindow.StartPage;
			ViewIsLoaded = true;
			return default;
		});
	}

	[PropertyChanged(nameof(StartPageUrl))]
	private ValueTask StartPageUrlChangedAsync(string value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		Debug.WriteLine($"Start Page Url Changed: {value}");
		// コンフィグのテーマ設定を更新
		ConfigManager.Config.BrowserWindow.StartPage = value;
		return default;
	}
}
