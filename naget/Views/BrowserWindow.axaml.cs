using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using naget.Models.BrowserHistory;
using naget.ViewModels;
using System;

namespace naget.Views;

public partial class BrowserWindow : Window
{
	public BrowserWindow()
	{
		InitializeComponent();

		NativeWebView webview = this.FindControl<NativeWebView>("webview")!;
		var vm = new BrowserWindowViewModel(webview);
		DataContext = vm;

		var addressBox = this.FindControl<TextBox>("AddressBox");
		addressBox!.GotFocus += (s, e) => vm.AddressBoxFocused = true;
		addressBox!.LostFocus += (s, e) => vm.AddressBoxFocused = false;

		var historyList = this.FindControl<ListBox>("HistoryList");
		historyList!.AddHandler(InputElement.TappedEvent, HistoryList_Tapped, RoutingStrategies.Bubble, handledEventsToo: true);
		historyList!.AddHandler(InputElement.KeyDownEvent, HistoryList_KeyDown, RoutingStrategies.Bubble, handledEventsToo: true);

		// ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
		Closing += (_, e) =>
		{
			Hide();
			e.Cancel = true;
		};
	}

	private void HistoryFlyout_Opened(object? sender, EventArgs e)
	{
		if (DataContext is BrowserWindowViewModel vm) vm.RefreshHistoryList();
	}

	private void HistoryList_Tapped(object? sender, TappedEventArgs e)
	{
		ActivateSelectedHistoryEntry();
	}

	private void HistoryList_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key is not (Key.Enter or Key.Space)) return;
		ActivateSelectedHistoryEntry();
	}

	private void ActivateSelectedHistoryEntry()
	{
		if (this.FindControl<ListBox>("HistoryList") is not { } list) return;
		if (list.SelectedItem is not BrowserHistoryEntry entry) return;

		if (DataContext is BrowserWindowViewModel vm) vm.OpenHistoryEntry(entry);

		list.SelectedItem = null;

		// ポインターの押下・解放処理が完了してから Flyout を閉じ、フォーカスを WebView に戻す
		Dispatcher.UIThread.Post(() =>
		{
			this.FindControl<Button>("HistoryButton")?.Flyout?.Hide();
			this.FindControl<NativeWebView>("webview")?.Focus();
		}, DispatcherPriority.Input);
	}
}
