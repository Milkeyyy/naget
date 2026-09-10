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
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace naget.ViewModels;

[ViewModel]
public class BrowserWindowViewModel
{
	public Well<Window> BrowserWindowWell { get; } = Well.Factory.Create<Window>();

	private const string SpaHookScript = """
		(function() {
			if (window.__nagetSpaHook) return;
			window.__nagetSpaHook = true;
			var post = function() {
				try {
					invokeCSharpAction(JSON.stringify({ url: location.href, title: document.title }));
				} catch (e) {}
			};
			['pushState', 'replaceState'].forEach(function(name) {
				var original = history[name];
				history[name] = function() {
					var result = original.apply(this, arguments);
					post();
					return result;
				};
			});
			window.addEventListener('popstate', post);
			window.addEventListener('hashchange', post);
			var lastTitle = document.title;
			var observer = new MutationObserver(function() {
				if (document.title !== lastTitle) {
					lastTitle = document.title;
					post();
				}
			});
			var titleElement = document.querySelector('title');
			if (titleElement) {
				observer.observe(titleElement, { childList: true, characterData: true, subtree: true });
			} else {
				observer.observe(document.documentElement, { childList: true, subtree: true });
			}
		})();
		""";

	public string WindowTitleText { get; private set; } = Resources.Window_InAppBrowser;
	public string WindowTitle
	{
		get
		{
			if (string.IsNullOrWhiteSpace(WindowTitleText))
			{
				// 渡されたタイトルが空の場合はデフォルトのタイトルにする
				return Resources.Window_InAppBrowser + " - " + App.ProductName;
			}
			else
			{
				return WindowTitleText;
			}
		}
		set { WindowTitleText = value; }
	}

	public WindowState WindowState { get; set; } = WindowState.Normal;
	public double Width { get; set; } = 1280;
	public double Height { get; set; } = 720;

	public bool WindowOpened { get; set; }

	public bool AddressBoxFocused { get; set; }

	private NativeWebView WebViewCtrl;

	private string beforeAddress = string.Empty;
	private string lastPageUrl = string.Empty;
	public bool WebViewCanGoBack { get; private set; }
	public bool WebViewCanGoForward { get; private set; }

	// アドレスバー用 (string)
	public string Address { get; set; }

	// NativeWebView の Source バインディング用 (Uri)
	public Uri? CurrentSource { get; set; }

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

	public Command ShowClearHistoryConfirmCommand { get; }

	public Command CancelClearHistoryCommand { get; }

	public Command ClearHistoryCommand { get; }

	public BrowserWindowViewModel(NativeWebView wb)
	{
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
			App.BrowserWindow.Position = HotKeyHelper.GetCenterScreen(App.BrowserWindow) ?? new(0,0);

			WindowOpened = true;

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

			// 開いているページのURLをリセット
			CurrentSource = new Uri("about:blank");

			return default;
		});

		WebViewCtrl = wb;

		// NativeWebView のイベント購読
		WebViewCtrl.NavigationCompleted += WebView_NavigationCompleted;
		WebViewCtrl.WebMessageReceived += WebView_WebMessageReceived;
		WebViewCtrl.PropertyChanged += WebViewOnPropertyChanged;

		// 閲覧データの保存先を設定する (コンフィグの設定が空欄の場合はコンフィグフォルダー配下)
		WebViewCtrl.EnvironmentRequested += (sender, e) =>
		{
			if (e is Avalonia.Platform.WindowsWebView2EnvironmentRequestedEventArgs webView2)
			{
				webView2.UserDataFolder = App.GetBrowserDataFolder();
			}
		};

#if DEBUG
		WebViewCtrl.EnvironmentRequested += (sender, e) =>
		{
			e.EnableDevTools = true;
		};
#endif

		Address = CurrentSource?.ToString() ?? string.Empty;
		lastPageUrl = Address;

		NavigateCommand = Command.Factory.Create(() =>
		{
			if (Uri.TryCreate(Address, UriKind.Absolute, out var uri))
			{
				CurrentSource = uri;
			}
			return default;
		});

		CutCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.TryGetCommandManager()?.Cut();
			return default;
		});

		CopyCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.TryGetCommandManager()?.Copy();
			return default;
		});

		PasteCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.TryGetCommandManager()?.Paste();
			return default;
		});

		UndoCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.TryGetCommandManager()?.Undo();
			return default;
		});

		RedoCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.TryGetCommandManager()?.Redo();
			return default;
		});

		SelectAllCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.TryGetCommandManager()?.SelectAll();
			return default;
		});

		DeleteCommand = Command.Factory.Create(async () =>
		{
			await WebViewCtrl.InvokeScript("document.execCommand('delete')");
		});

		BackCommand = Command.Factory.Create(() =>
		{
			WebView_GoBack();
			return default;
		});

		ForwardCommand = Command.Factory.Create(() =>
		{
			WebView_GoForward();
			return default;
		});

		ReloadCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.Refresh();
			return default;
		});

		// 既定のブラウザーで開く (信頼境界のため http/https のみ許可)
		OpenInDefaultBrowserCommand = Command.Factory.Create(() =>
		{
			if (Uri.TryCreate(lastPageUrl, UriKind.Absolute, out Uri? uri) &&
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
				App.Logger.Debug("Open in default browser: Invalid URL: " + lastPageUrl);
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

	[PropertyChanged(nameof(CurrentSource))]
	private ValueTask OnCurrentSourceChangedAsync(Uri? value)
	{
		App.Logger.Debug("CurrentSource Changed: " + value);

		lastPageUrl = value?.ToString() ?? lastPageUrl;
		if (!AddressBoxFocused) Address = value?.ToString() ?? string.Empty;

		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;
		App.Logger.Debug($" - {WebViewCanGoBack} {WebViewCanGoForward}");

		return default;
	}

	private void WebViewOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
		App.Logger.Debug("WebView PropertyChanged: " + e.Property.Name);

		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;
		App.Logger.Debug($" - {WebViewCanGoBack} {WebViewCanGoForward}");
	}

	private async void WebView_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
	{
		App.Logger.Debug("WebView NavigationCompleted: " + WebViewCtrl.Source);

		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;

		// SPA 遷移を検出するための JS フックを注入する
		try
		{
			await WebViewCtrl.InvokeScript(SpaHookScript);
		}
		catch (Exception ex)
		{
			App.Logger.Debug("SPA hook injection failed: " + ex.Message);
		}

		// アドレスバーを更新
		lastPageUrl = WebViewCtrl.Source?.ToString() ?? lastPageUrl;
		if (!AddressBoxFocused) Address = WebViewCtrl.Source?.ToString() ?? string.Empty;
		if (WebViewCtrl.Source != null) CurrentSource = WebViewCtrl.Source;

		// ウィンドウタイトルを更新する (NativeWebView には Title プロパティがないため JS で取得)
		try
		{
			string? title = await WebViewCtrl.InvokeScript("document.title");
			// WebView2 の InvokeScript は JSON エンコードされた結果を返すためデコードする (macOS は生の文字列)
			try
			{
				title = JsonSerializer.Deserialize<string>(title ?? string.Empty);
			}
			catch (JsonException)
			{
			}
			WindowTitle = title ?? string.Empty;
		}
		catch
		{
			WindowTitle = string.Empty;
		}

		BrowserHistoryManager.Add(lastPageUrl, WindowTitleText);

		App.Logger.Debug($" - {WebViewCanGoBack} {WebViewCanGoForward}");
	}

	// アドレスバーのフォーカスが外れたら、入力内容を正規のページURLに戻す
	[PropertyChanged(nameof(AddressBoxFocused))]
	private ValueTask OnAddressBoxFocusedChangedAsync(bool value)
	{
		if (!value) Address = lastPageUrl;
		return default;
	}

	private void WebView_WebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.Body)) return;
		try
		{
			using JsonDocument document = JsonDocument.Parse(e.Body);
			JsonElement root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return;
			if (!root.TryGetProperty("url", out JsonElement urlElement) || urlElement.ValueKind != JsonValueKind.String) return;
			if (!root.TryGetProperty("title", out JsonElement titleElement) || titleElement.ValueKind != JsonValueKind.String) return;

			string url = urlElement.GetString() ?? string.Empty;
			string title = titleElement.GetString() ?? string.Empty;

			App.Logger.Debug("WebView WebMessage: " + url + " / " + title);

			lastPageUrl = url;
			if (!AddressBoxFocused) Address = url;
			WindowTitle = title;

			WebViewCanGoBack = WebViewCtrl.CanGoBack;
			WebViewCanGoForward = WebViewCtrl.CanGoForward;

			BrowserHistoryManager.Add(url, title);
		}
		catch (JsonException)
		{
		}
	}

	private void WebView_GoBack()
	{
		beforeAddress = WebViewCtrl.Source?.ToString() ?? string.Empty;
		WebViewCtrl.GoBack();
		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;
	}

	private void WebView_GoForward()
	{
		beforeAddress = Address;
		WebViewCtrl.GoForward();
		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;
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
			WebViewCtrl.Navigate(uri);
		}
	}

	[PropertyChanged(nameof(HistoryFilter))]
	private ValueTask OnHistoryFilterChangedAsync(string value)
	{
		RefreshHistoryList();
		return default;
	}
}
