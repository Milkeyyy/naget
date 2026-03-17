using Avalonia.Controls;
using naget.ViewModels;
using System;
using System.Diagnostics;

namespace naget.Views;

public partial class SettingsWindow : Window
{
	public SettingsWindow()
	{
		InitializeComponent();

		DataContext = new SettingsWindowViewModel();

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (s, e) =>
		{
			((Window)s).Hide();
			e.Cancel = true;
		};

		var listBox = this.FindControl<ListBox>("navigationMenu");
		listBox.SelectionChanged += OnNavigationMenuSelectionChanged;
		listBox.SelectedIndex = 0;
	}

	private void OnNavigationMenuSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (sender is not ListBox listBox) return;
		if (listBox.SelectedItem is not ListBoxItem item) return;
		if (item.Tag == null) return;

		string tag = item.Tag.ToString()!;
		App.Logger.Debug($"Navigation Selected: {tag}");

		string smpPage = $"naget.Views.Settings.{tag}";
		App.Logger.Debug($"- Page: {smpPage}");

		var pg = Activator.CreateInstance(Type.GetType(smpPage)!);
		var contentArea = this.FindControl<ContentControl>("contentArea");
		if (contentArea != null)
		{
			contentArea.Content = pg;
		}
	}
}
