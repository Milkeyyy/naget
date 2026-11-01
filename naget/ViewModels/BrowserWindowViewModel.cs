using Avalonia;
using Avalonia.Controls;
using Epoxy;
using naget.Assets.Locales;
using naget.Helpers;
using naget.Models.BrowserHistory;
using naget.Models.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace naget.ViewModels;

[ViewModel]
public class BrowserWindowViewModel
{
	public Well<Window> BrowserWindowWell { get; } = Well.Factory.Create<Window>();

	public string WindowTitleText { get; private set; } = Resources.Window_InAppBrowser;

	public WindowState WindowState { get; set; } = WindowState.Normal;
	public double Width { get; set; } = 1280;
	public double Height { get; set; } = 720;

	public bool AddressBoxFocused { get; set; }

	private readonly Panel webViewHost;

	public ObservableCollection<BrowserTabViewModel> Tabs { get; } = [];

	public BrowserTabViewModel? SelectedTab { get; set; }

	public bool WebViewCanGoBack { get; private set; }
	public bool WebViewCanGoForward { get; private set; }

	public string Address { get; set; } = string.Empty;

	public ObservableCollection<BrowserHistoryEntry> HistoryEntries { get; } = [];

	public string HistoryFilter { get; set; } = string.Empty;

	public bool HasHistoryEntries { get; private set; }

	public bool IsHistoryListEmpty { get; private set; } = true;

	public bool IsHistoryClearButtonVisible { get; private set; } = true;

	public bool IsClearHistoryConfirmVisible { get; private set; }

	public Command NavigateCommand { get; }

	public Command CutCommand { get; }

	public Command CopyCommand { get; }

	public Command PasteCommand { get; }

	public Command UndoCommand { get; }

	public Command RedoCommand { get; }

	public Command SelectAllCommand { get; }

	public Command DeleteCommand { get; }

	public Command BackCommand { get; }

	public Command ForwardCommand { get; }

	public Command ReloadCommand { get; }

	public Command OpenInDefaultBrowserCommand { get; }

	public Command NewTabCommand { get; }

	public Command ShowClearHistoryConfirmCommand { get; }

	public Command CancelClearHistoryCommand { get; }

	public Command ClearHistoryCommand { get; }

	public BrowserWindowViewModel(Panel host)
	{
		webViewHost = host;

		// ウィンドウが開かれた時のイベント
		BrowserWindowWell.Add(Window.WindowOpenedEvent, () =>
		{
			App.Logger.Debug("BrowserWindow Opened");

			// ウィンドウの設定を読み込む
			Width = ConfigManager.Config.BrowserWindow.Width;
			Height = ConfigManager.Config.BrowserWindow.Height;
			if (ConfigManager.Config.BrowserWindow.State == WindowState.Minimized) WindowState = WindowState.Normal;
			else WindowState = ConfigManager.Config.BrowserWindow.State;

			// ウィンドウをマウスカーソルが存在するディスプレイの中央へ移動する
			PixelPoint? centerPos = HotKeyHelper.GetCenterScreen(App.BrowserWindow);
			if (centerPos is PixelPoint pos)
			{
				App.BrowserWindow.Position = pos;
			}

			return default;
		});

		// ウィンドウが閉じられた時のイベント
		BrowserWindowWell.Add<WindowClosingEventArgs>("Closing", e =>
		{
			App.Logger.Debug("BrowserWindow Closed");

			// ウィンドウの設定を保存する
			if (WindowState == WindowState.Normal)
			{
				ConfigManager.Config.BrowserWindow.Width = Width;
				ConfigManager.Config.BrowserWindow.Height = Height;
			}
			ConfigManager.Config.BrowserWindow.State = WindowState;

			foreach (BrowserTabViewModel tab in Tabs.ToArray())
			{
				CloseTab(tab);
			}

			return default;
		});

		NavigateCommand = Command.Factory.Create(() =>
		{
			if (Uri.TryCreate(Address, UriKind.Absolute, out Uri? uri))
			{
				SelectedTab?.Navigate(uri);
			}
			return default;
		});

		CutCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.WebView.TryGetCommandManager()?.Cut();
			return default;
		});

		CopyCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.WebView.TryGetCommandManager()?.Copy();
			return default;
		});

		PasteCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.WebView.TryGetCommandManager()?.Paste();
			return default;
		});

		UndoCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.WebView.TryGetCommandManager()?.Undo();
			return default;
		});

		RedoCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.WebView.TryGetCommandManager()?.Redo();
			return default;
		});

		SelectAllCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.WebView.TryGetCommandManager()?.SelectAll();
			return default;
		});

		DeleteCommand = Command.Factory.Create(async () =>
		{
			if (SelectedTab is not null)
			{
				await SelectedTab.WebView.InvokeScript("document.execCommand('delete')");
			}
		});

		BackCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.GoBack();
			return default;
		});

		ForwardCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.GoForward();
			return default;
		});

		ReloadCommand = Command.Factory.Create(() =>
		{
			SelectedTab?.Refresh();
			return default;
		});

		// 既定のブラウザーで開く (信頼境界のため http/https のみ許可)
		OpenInDefaultBrowserCommand = Command.Factory.Create(() =>
		{
			string url = SelectedTab?.Url ?? string.Empty;
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
				(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
			{
				App.Logger.Debug("Open in default browser: " + uri);
				Process.Start(new ProcessStartInfo(uri.ToString())
				{
					UseShellExecute = true
				});
			}
			else
			{
				App.Logger.Debug("Open in default browser: Invalid URL: " + url);
			}
			return default;
		});

		NewTabCommand = Command.Factory.Create(() =>
		{
			if (Uri.TryCreate(ConfigManager.Config.BrowserWindow.StartPage, UriKind.Absolute, out Uri? uri))
			{
				OpenNewTab(uri);
			}
			return default;
		});

		ShowClearHistoryConfirmCommand = Command.Factory.Create(() =>
		{
			IsClearHistoryConfirmVisible = true;
			IsHistoryClearButtonVisible = false;
			return default;
		});

		CancelClearHistoryCommand = Command.Factory.Create(() =>
		{
			IsClearHistoryConfirmVisible = false;
			IsHistoryClearButtonVisible = true;
			return default;
		});

		ClearHistoryCommand = Command.Factory.Create(() =>
		{
			App.Logger.Debug("Clear Browser History");
			BrowserHistoryManager.Clear();
			RefreshHistoryList();
			return default;
		});
	}

	public void OpenNewTab(Uri uri)
	{
		BrowserTabViewModel tab = new(uri, OpenNewTab);
		tab.PropertyChanged += Tab_PropertyChanged;
		tab.WebView.IsVisible = false;
		webViewHost.Children.Add(tab.WebView);
		Tabs.Add(tab);
		SelectedTab = tab;
	}

	public void CloseTab(BrowserTabViewModel tab)
	{
		int index = Tabs.IndexOf(tab);
		if (index < 0) return;

		tab.PropertyChanged -= Tab_PropertyChanged;
		tab.Unload();
		Tabs.Remove(tab);
		webViewHost.Children.Remove(tab.WebView);

		if (Tabs.Count == 0)
		{
			SelectedTab = null;
			Address = string.Empty;
			WindowTitleText = Resources.Window_InAppBrowser;
			WebViewCanGoBack = false;
			WebViewCanGoForward = false;
			App.BrowserWindow.Hide();
			return;
		}

		if (SelectedTab is null || !Tabs.Contains(SelectedTab))
		{
			SelectedTab = Tabs[Math.Min(index, Tabs.Count - 1)];
		}
	}

	[PropertyChanged(nameof(SelectedTab))]
	private ValueTask OnSelectedTabChangedAsync(BrowserTabViewModel? value)
	{
		foreach (BrowserTabViewModel tab in Tabs)
		{
			tab.WebView.IsVisible = ReferenceEquals(tab, value);
		}

		if (value is null) return default;

		if (!AddressBoxFocused) Address = value.Url;
		WindowTitleText = value.Title;
		WebViewCanGoBack = value.CanGoBack;
		WebViewCanGoForward = value.CanGoForward;
		value.WebView.Focus();

		return default;
	}

	private void Tab_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender is not BrowserTabViewModel tab) return;
		if (!ReferenceEquals(tab, SelectedTab)) return;

		if (!AddressBoxFocused) Address = tab.Url;
		WindowTitleText = tab.Title;
		WebViewCanGoBack = tab.CanGoBack;
		WebViewCanGoForward = tab.CanGoForward;
	}

	// アドレスバーのフォーカスが外れたら、入力内容を正規のページURLに戻す
	[PropertyChanged(nameof(AddressBoxFocused))]
	private ValueTask OnAddressBoxFocusedChangedAsync(bool value)
	{
		if (!value) Address = SelectedTab?.Url ?? string.Empty;
		return default;
	}

	public void RefreshHistoryList()
	{
		HistoryEntries.Clear();

		IEnumerable<BrowserHistoryEntry> query = BrowserHistoryManager.Entries;
		if (!string.IsNullOrWhiteSpace(HistoryFilter))
		{
			query = query.Where(e =>
				e.Url.Contains(HistoryFilter, StringComparison.OrdinalIgnoreCase) ||
				e.Title.Contains(HistoryFilter, StringComparison.OrdinalIgnoreCase));
		}

		foreach (BrowserHistoryEntry entry in query)
		{
			HistoryEntries.Add(entry);
		}

		HasHistoryEntries = HistoryEntries.Count > 0;
		IsHistoryListEmpty = !HasHistoryEntries;
		IsClearHistoryConfirmVisible = false;
		IsHistoryClearButtonVisible = true;
	}

	public void OpenHistoryEntry(BrowserHistoryEntry entry)
	{
		if (Uri.TryCreate(entry.Url, UriKind.Absolute, out Uri? uri))
		{
			App.Logger.Debug("Open History Entry: " + entry.Url);
			SelectedTab?.Navigate(uri);
		}
	}

	[PropertyChanged(nameof(HistoryFilter))]
	private ValueTask OnHistoryFilterChangedAsync(string value)
	{
		RefreshHistoryList();
		return default;
	}
}
