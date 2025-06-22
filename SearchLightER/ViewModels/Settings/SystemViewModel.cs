using Avalonia.Controls;
using Epoxy;
using naget.Models.Config;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;

namespace naget.ViewModels.Settings;

[ViewModel]
public class SystemViewModel
{
	public Well<UserControl> SystemViewWell { get; } = Well.Factory.Create<UserControl>();

	public static ReadOnlyCollection<Language> LanguageList => ConfigClass.LanguageList;
	public Language SelectedLanguage { get; set; } = LanguageList[0];
	public bool LanguageNoteTextIsVisible { get; set; }

	private bool ViewIsLoaded;

	public SystemViewModel()
	{
		// ビューがロードされた時の処理
		SystemViewWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("SystemView Loaded");
			// コンフィグの言語設定から該当する言語を取得して選択中言語に設定する
			SelectedLanguage = LanguageList.FirstOrDefault(l => l.Code == ConfigManager.Config.Language) ?? LanguageList[0];
			// ローカライズ設定の言語と選択リストの言語が異なる場合は注意書きテキストを表示する
			LanguageNoteTextIsVisible = SelectedLanguage.Code != Assets.Locales.Resources.Culture.Name;
			ViewIsLoaded = true;
			return default;
		});
	}

	[PropertyChanged(nameof(SelectedLanguage))]
	private ValueTask SelectedLanguageChangedAsync(Language value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		Debug.WriteLine($"Selected Language Changed: {value.DisplayName} ({value.Code})");
		// ローカライズ設定の言語と選択リストの言語が異なる場合は注意書きテキストを表示する
		LanguageNoteTextIsVisible = value.Code != Assets.Locales.Resources.Culture.Name;
		// コンフィグの言語設定を更新
		ConfigManager.Config.Language = value.Code;
		return default;
	}
}
