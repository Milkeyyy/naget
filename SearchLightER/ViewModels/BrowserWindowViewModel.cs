using Avalonia;
using Avalonia.Controls;
using Epoxy;
using naget.Assets.Locales;
using naget.Helpers;
using naget.Models.Config;
using System.Diagnostics;
using System.Threading.Tasks;
using WebViewControl;

namespace naget.ViewModels;

[ViewModel]
public class BrowserWindowViewModel
{
	public Well<Window> BrowserWindowWell { get; } = Well.Factory.Create<Window>();

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

	private WebView WebViewCtrl;

	private string beforeAddress = string.Empty;
	public bool WebViewCanGoBack { get; private set; }
	public bool WebViewCanGoForward { get; private set; }

	public string Address { get; set; }

	public string CurrentAddress { get; set; }

	public Command NavigateCommand { get; }

	public Command ShowDevToolsCommand { get; }

	public Command CutCommand { get; }

	public Command CopyCommand { get; }

	public Command PasteCommand { get; }

	public Command UndoCommand { get; }

	public Command RedoCommand { get; }

	public Command SelectAllCommand { get; }

	public Command DeleteCommand { get; }

	public Command BackCommand { get; }

	public Command ForwardCommand { get; }

	public BrowserWindowViewModel(WebView wb)
	{
		// ウィンドウが開かれた時のイベント
		BrowserWindowWell.Add(Window.WindowOpenedEvent, () =>
		{
			Debug.WriteLine("BrowserWindow Opened");
			
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
			Debug.WriteLine("BrowserWindow Closed");

			// ウィンドウの設定を保存する
			if (WindowState == WindowState.Normal)
			{
				ConfigManager.Config.BrowserWindow.Width = Width;
				ConfigManager.Config.BrowserWindow.Height = Height;
			}
			ConfigManager.Config.BrowserWindow.State = WindowState;

			// 開いているページのURLをリセット
			CurrentAddress = "about:blank";

			return default;
		});

		WebViewCtrl = wb;
		WebViewCtrl.Navigated += WebView_Navigated;
		WebViewCtrl.PropertyChanged += WebViewOnPropertyChanged;
		Address = CurrentAddress;

		NavigateCommand = Command.Factory.Create(() =>
		{
			CurrentAddress = Address;
			return default;
		});

		ShowDevToolsCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.ShowDeveloperTools();
			return default;
		});

		CutCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.Cut();
			return default;
		});

		CopyCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.Copy();
			return default;
		});

		PasteCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.Paste();
			return default;
		});

		UndoCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.Undo();
			return default;
		});

		RedoCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.Redo();
			return default;
		});

		SelectAllCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.SelectAll();
			return default;
		});

		DeleteCommand = Command.Factory.Create(() =>
		{
			WebViewCtrl.EditCommands.Delete();
			return default;
		});

		BackCommand = Command.Factory.Create(() =>
		{
			WebView_GoBack();
			return default;
			/*,
			this.WhenAnyValue(
				x => x.WebViewCanGoBack
			)*/
		});

		ForwardCommand = Command.Factory.Create(() =>
		{
			WebView_GoForward();
			return default;
			/*,
			this.WhenAnyValue(
				x => x.WebViewCanGoForward
			)*/
		});
	}

	[PropertyChanged(nameof(WebViewCtrl.CanGoBack))]
	private ValueTask WebViewCanGoBackChanged(bool value)
	{
		Debug.WriteLine("WebView CanGoBack Changed: " + value);
		return default;
	}

	[PropertyChanged(nameof(WebViewCtrl.CanGoForward))]
	private ValueTask WebViewCanGoForwardChanged(bool value)
	{
		Debug.WriteLine("WebView CanGoForward Changed: " + value);
		return default;
	}

	[PropertyChanged(nameof(CurrentAddress))]
	private ValueTask OnCurrentAddressChangedAsync(string value)
	{
		Debug.WriteLine("CurrentAddress Changed: " + value);

		Address = value;

		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;
		Debug.WriteLine($" - {WebViewCanGoBack} {WebViewCanGoForward}");

		// ウィンドウタイトルを更新する
		WindowTitle = WebViewCtrl.Title;

		return default;
	}

	private void WebViewOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
		Debug.WriteLine("WebView PropertyChanged: " + e.Property.Name);

		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;
		Debug.WriteLine($" - {WebViewCanGoBack} {WebViewCanGoForward}");

		// ウィンドウタイトルを更新する
		WindowTitle = WebViewCtrl.Title;
	}

	private void WebView_Navigated(string url, string frameName)
	{
		Debug.WriteLine("WebView Navigated: " + url + " | " + frameName);
		//Debug.WriteLine("- Update Property");
		//Debug.WriteLine($" - {webview.CanGoBack} {webview.CanGoForward}");

		WebViewCanGoBack = WebViewCtrl.CanGoBack;
		WebViewCanGoForward = WebViewCtrl.CanGoForward;

		// ウィンドウタイトルを更新する
		WindowTitle = WebViewCtrl.Title;

		Debug.WriteLine($" - {WebViewCanGoBack} {WebViewCanGoForward}");
	}

	private void WebView_GoBack()
	{
		beforeAddress = WebViewCtrl.Address;
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

	/*public bool WebViewCanGoBack
	{
		get => webviewCanGoBack;
		set => this.RaiseAndSetIfChanged(ref webviewCanGoBack, value);
	}

	public bool WebViewCanGoForward
	{
		get => webviewCanGoForward;
		set => this.RaiseAndSetIfChanged(ref webviewCanGoForward, value);
	}*/
}
