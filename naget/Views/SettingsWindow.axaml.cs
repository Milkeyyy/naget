using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using naget.ViewModels;
using System;
using System.Diagnostics;

namespace naget.Views;

public partial class SettingsWindow : AppWindow
{
	public SettingsWindow()
	{
		InitializeComponent();

		DataContext = new SettingsWindowViewModel();

		TitleBar.ExtendsContentIntoTitleBar = true;

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			e.Cancel = true;
		};

		var nv = this.FindControl<NavigationView>("navigationMenu");
		nv.SelectionChanged += OnNavigationMenuSelectionChanged;
		nv.SelectedItem = nv.MenuItems.ElementAt(0);
	}

	private void OnNavigationMenuSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
	{
		string smpPage;
		object pg;

		if (e.SelectedItem is NavigationViewItem nvi)
		{
			if (nvi.Tag == null) return; // タグがnullの場合は何もしない

			App.Logger.Debug($"Navigation Selected: {nvi.Tag}");

			/*// 設定が選択された場合はアプリケーション設定のページを表示する
			if ((string)nvi.Tag == "Settings") { smpPage = "naget.Views.Settings.AppSettingsView"; }
			// それ以外は指定されたタグに基づいてページを表示する
			else { smpPage = $"naget.Views.Settings.{nvi.Tag}"; }*/

			smpPage = $"naget.Views.Settings.{nvi.Tag}";

			App.Logger.Debug($"- Page: {smpPage}");

			pg = Activator.CreateInstance(Type.GetType(smpPage));
			(sender as NavigationView).Content = pg;
		}
	}
}
