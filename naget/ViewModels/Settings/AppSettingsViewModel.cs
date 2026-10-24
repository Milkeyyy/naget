using Avalonia.Controls;
using Epoxy;
using naget.Helpers;
using naget.Models.Config;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class AppSettingsViewModel
{
	private bool ViewIsLoaded;

	public Well<UserControl> ViewWell { get; } = Well.Factory.Create<UserControl>();

	public bool LanguageNoteTextIsVisible { get; set; }
	public Command RestartAppCommand { get; }

	#region テーマ設定関連
	public static ReadOnlyCollection<string> ThemeList => ConfigClass.ThemeList;
	public string SelectedTheme { get; set; } = string.Empty;
	#endregion

	#region 言語設定関連
	public static ReadOnlyCollection<Language> LanguageList => ConfigClass.LanguageList;
	public Language SelectedLanguage { get; set; } = LanguageList[0];
	#endregion

	#region スタートアップ関連
	public bool StartupEnabled { get; set; }
	public bool StartupBusy { get; set; } = true;
	public bool StartupErrorIsVisible { get; set; }
	#endregion

	#region アップデート関連
	public Command CheckUpdateCommand { get; }
	#endregion

	public Command AboutClickCommand { get; }

	public AppSettingsViewModel()
	{
		// ビューがロードされた時の処理
		ViewWell.Add(Control.LoadedEvent, async () =>
		{
			App.Logger.Debug("AppSettingsView Loaded");

			ViewIsLoaded = false;

			// テーマ
			// コンフィグのテーマ設定から該当するテーマを取得して選択中テーマに設定する
			SelectedTheme = ThemeList.FirstOrDefault(t => t == ConfigManager.Config.Theme) ?? ThemeList[0];

			// 言語
			// コンフィグの言語設定から該当する言語を取得して選択中言語に設定する
			SelectedLanguage = LanguageList.FirstOrDefault(l => l.Code == ConfigManager.Config.Language) ?? LanguageList[0];
			// ローカライズ設定の言語と選択リストの言語が異なる場合は注意書きテキストを表示する
			LanguageNoteTextIsVisible = SelectedLanguage.Code != Assets.Locales.Resources.Culture.Name;

			// スタートアップ
			StartupBusy = true;
			StartupEnabled = await StartupManager.GetEnabledAsync();
			StartupBusy = false;

			ViewIsLoaded = true;
		});

		// アプリケーション再起動コマンド
		RestartAppCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Execute Restart App Command");
			App.RestartApplication();
			return default;
		});

		// アップデートチェックコマンド
		CheckUpdateCommand = Command.Factory.Create(async () =>
		{
			App.Logger.Debug("Execute Check Update Command");
			await App.Updater.ManualCheck(true);
		});

		// アプリケーション情報表示コマンド
		AboutClickCommand = Command.Factory.Create(() =>
		{
			App.AboutWindow!.ShowDialog(App.SettingsWindow!);
			return default;
		});
	}

	/// <summary>
	/// テーマが変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(SelectedTheme))]
	private ValueTask SelectedThemeChanged(string value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		App.Logger.Debug($"Selected Theme Changed: {value}");
		// アプリ全体のテーマを変更
		App.ChangeTheme(value);
		// コンフィグのテーマ設定を更新
		ConfigManager.Config.Theme = value;
		return default;
	}

	/// <summary>
	/// 言語が変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(SelectedLanguage))]
	private ValueTask SelectedLanguageChanged(Language value)
	{
		if (!ViewIsLoaded) return default; // ビューがロードされていない場合は処理をスキップ
		App.Logger.Debug($"Selected Language Changed: {value.DisplayName} ({value.Code})");
		// ローカライズ設定の言語と選択リストの言語が異なる場合は注意書きテキストを表示する
		LanguageNoteTextIsVisible = value.Code != Assets.Locales.Resources.Culture.Name;
		// コンフィグの言語設定を更新
		ConfigManager.Config.Language = value.Code;
		return default;
	}

	/// <summary>
	/// スタートアップ設定が変更された時の処理
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[PropertyChanged(nameof(StartupEnabled))]
	private async ValueTask StartupEnabledChanged(bool value)
	{
		if (!ViewIsLoaded || StartupBusy) return;
		App.Logger.Debug($"Selected Startup Changed: {value}");
		StartupBusy = true;
		try
		{
			if (!await StartupManager.SetEnabledAsync(value))
			{
				StartupErrorIsVisible = true;
				StartupEnabled = !value;
			}
			else
			{
				StartupErrorIsVisible = false;
			}
		}
		finally
		{
			StartupBusy = false;
		}
	}
}
