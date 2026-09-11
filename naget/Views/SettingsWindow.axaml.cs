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
		Closing += (_, e) =>
		{
			Hide();
			e.Cancel = true;
		};

		NavigationView nv = this.FindControl<NavigationView>("navigationMenu")!;
		nv.SelectionChanged += OnNavigationMenuSelectionChanged;
		nv.SelectedItem = nv.MenuItems.ElementAt(0);
	}

	private void OnNavigationMenuSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
	{
		string smpPage;
		object? pg;

		if (e.SelectedItem is NavigationViewItem nvi)
		{
			if (nvi.Tag == null) return; // タグがnullの場合は何もしない

			App.Logger.Debug($"Navigation Selected: {nvi.Tag}");

			smpPage = $"naget.Views.Settings.{nvi.Tag}";

			App.Logger.Debug($"- Page: {smpPage}");

			Type? pageType = Type.GetType(smpPage);
			if (pageType == null) return;
			pg = Activator.CreateInstance(pageType);
			if (pg == null) return;
			if (sender is NavigationView navigationView) navigationView.Content = pg;
		}
	}
}
