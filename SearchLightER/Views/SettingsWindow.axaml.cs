using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using System;

namespace SearchLight.Views;

public partial class SettingsWindow : AppWindow
{
	public SettingsWindow()
	{
		InitializeComponent();

		//DataContext = new SettingsShortcutKeyViewModel();

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
		if (e.SelectedItem is NavigationViewItem nvi)
		{
			var smpPage = $"SearchLight.Views.Settings.{nvi.Tag}";
			var pg = Activator.CreateInstance(Type.GetType(smpPage));
			(sender as NavigationView).Content = pg;
		}
	}
}
