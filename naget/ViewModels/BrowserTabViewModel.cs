using Avalonia;
using Avalonia.Controls;
using Epoxy;
using naget.Assets.Locales;
using naget.Models.BrowserHistory;
using naget.Models.Config;
using System;
using System.ComponentModel;
using System.Text.Json;

namespace naget.ViewModels;

public class BrowserTabViewModel : INotifyPropertyChanged
{
	public NativeWebView WebView { get; }

	private string url = string.Empty;
	public string Url
	{
		get { return url; }
		private set
		{
			if (url == value) return;
			url = value;
			RaisePropertyChanged(nameof(Url));
		}
	}

	private string title = Resources.Window_InAppBrowser;
	public string Title
	{
		get { return title; }
		private set
		{
			if (title == value) return;
			title = value;
			RaisePropertyChanged(nameof(Title));
		}
	}

	private bool isUnloading;

	private bool canGoBack;
	public bool CanGoBack
	{
		get { return canGoBack; }
		private set
		{
			if (canGoBack == value) return;
			canGoBack = value;
			RaisePropertyChanged(nameof(CanGoBack));
		}
	}

	private bool canGoForward;
	public bool CanGoForward
	{
		get { return canGoForward; }
		private set
		{
			if (canGoForward == value) return;
			canGoForward = value;
			RaisePropertyChanged(nameof(CanGoForward));
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private const string SpaHookScript = """
		(function() {
			if (window.__nagetSpaHook) return;
			window.__nagetSpaHook = true;
			var lastTitle = document.title;
			var lastUrl = location.href;
			var lastSentTitle = lastTitle;
			var titleCache = {};
			titleCache[lastUrl] = lastTitle;
			var send = function() {
				var url = location.href;
				var domTitle = document.title;
				var title;
				if (domTitle !== lastTitle) {
					title = domTitle;
					titleCache[url] = domTitle;
				} else if (url === lastUrl) {
					title = lastSentTitle;
				} else if (titleCache[url]) {
					title = titleCache[url];
				} else {
					title = domTitle;
				}
				lastTitle = domTitle;
				lastUrl = url;
				lastSentTitle = title;
				try {
					invokeCSharpAction(JSON.stringify({ url: url, title: title }));
				} catch (e) {}
			};
			['pushState', 'replaceState'].forEach(function(name) {
				var original = history[name];
				history[name] = function() {
					var result = original.apply(this, arguments);
					send();
					return result;
				};
			});
			window.addEventListener('popstate', send);
			window.addEventListener('hashchange', send);
			var observer = new MutationObserver(function() {
				if (document.title !== lastTitle) {
					send();
				}
			});
			observer.observe(document.head || document.documentElement, { childList: true, subtree: true, characterData: true });
		})();
		""";

	public BrowserTabViewModel(Uri? initialUri, Action<Uri> newTabRequested)
	{
		WebView = new NativeWebView
		{
			Focusable = true
		};

		if (initialUri is not null) Url = initialUri.ToString();

		WebView.NavigationCompleted += WebView_NavigationCompleted;
		WebView.WebMessageReceived += WebView_WebMessageReceived;
		WebView.PropertyChanged += WebViewOnPropertyChanged;
		WebView.NewWindowRequested += (sender, e) =>
		{
			if (e.Request is Uri uri)
			{
				e.Handled = true;
				newTabRequested(uri);
			}
		};

		// 閲覧データの保存先を設定する (コンフィグの設定が空欄の場合はコンフィグフォルダー配下)
		WebView.EnvironmentRequested += (sender, e) =>
		{
			if (e is Avalonia.Platform.WindowsWebView2EnvironmentRequestedEventArgs webView2)
			{
				webView2.UserDataFolder = App.GetBrowserDataFolder();
				webView2.Language = ConfigManager.Config.Language;
			}
			else if (e is Avalonia.Platform.AppleWKWebViewEnvironmentRequestedEventArgs apple)
			{
				apple.ApplicationNameForUserAgent = App.ProductName + "/" + App.ProductVersion + " Safari/605.1.15";
			}
		};

#if DEBUG
		WebView.EnvironmentRequested += (sender, e) =>
		{
			e.EnableDevTools = true;
		};
#endif

		WebView.AdapterCreated += (sender, e) =>
		{
			if (OperatingSystem.IsWindows() && WebView.UserAgent is string userAgent)
			{
				WebView.UserAgent = userAgent + " " + App.ProductName + "/" + App.ProductVersion;
			}
		};

		if (initialUri is not null) WebView.Navigate(initialUri);
	}

	public void Navigate(Uri uri)
	{
		WebView.Navigate(uri);
	}

	public void GoBack()
	{
		WebView.GoBack();
		CanGoBack = WebView.CanGoBack;
		CanGoForward = WebView.CanGoForward;
	}

	public void GoForward()
	{
		WebView.GoForward();
		CanGoBack = WebView.CanGoBack;
		CanGoForward = WebView.CanGoForward;
	}

	public void Refresh()
	{
		WebView.Refresh();
	}

	public void Unload()
	{
		isUnloading = true;
		WebView.NavigateToString(string.Empty);
	}

	private void WebViewOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		CanGoBack = WebView.CanGoBack;
		CanGoForward = WebView.CanGoForward;
	}

	private async void WebView_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
	{
		if (isUnloading) return;

		App.Logger.Debug("WebView NavigationCompleted: " + WebView.Source);

		CanGoBack = WebView.CanGoBack;
		CanGoForward = WebView.CanGoForward;

		// SPA 遷移を検出するための JS フックを注入する
		try
		{
			await WebView.InvokeScript(SpaHookScript);
		}
		catch (Exception ex)
		{
			App.Logger.Debug("SPA hook injection failed: " + ex.Message);
		}

		Url = WebView.Source?.ToString() ?? Url;

		// タブのタイトルを更新する (NativeWebView には Title プロパティがないため JS で取得)
		string title = string.Empty;
		try
		{
			string? script = await WebView.InvokeScript("document.title");
			// WebView2 の InvokeScript は JSON エンコードされた結果を返すためデコードする (macOS は生の文字列)
			try
			{
				script = JsonSerializer.Deserialize<string>(script ?? string.Empty);
			}
			catch (JsonException)
			{
			}
			title = script ?? string.Empty;
		}
		catch
		{
		}

		Title = string.IsNullOrWhiteSpace(title) ? Resources.Window_InAppBrowser : title;

		BrowserHistoryManager.Add(Url, title);
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

			Url = url;
			Title = string.IsNullOrWhiteSpace(title) ? Resources.Window_InAppBrowser : title;

			CanGoBack = WebView.CanGoBack;
			CanGoForward = WebView.CanGoForward;

			BrowserHistoryManager.Add(url, title);
		}
		catch (JsonException)
		{
		}
	}

	private void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
