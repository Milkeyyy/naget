using Avalonia;
using Epoxy;
using System.Diagnostics;
using System.Threading.Tasks;
using WebViewControl;

namespace SearchLight.ViewModels;

[ViewModel]
public class BrowserWindowViewModel
{
	private WebView webview;

	private string address = string.Empty;
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
		webview = wb;
		webview.Navigated += WebView_Navigated;
		Address = CurrentAddress = "http://www.google.com/";

		NavigateCommand = Command.Factory.Create(() =>
		{
			CurrentAddress = Address;
			return default;
		});

		ShowDevToolsCommand = Command.Factory.Create(() =>
		{
			webview.ShowDeveloperTools();
			return default;
		});

		CutCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.Cut();
			return default;
		});

		CopyCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.Copy();
			return default;
		});

		PasteCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.Paste();
			return default;
		});

		UndoCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.Undo();
			return default;
		});

		RedoCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.Redo();
			return default;
		});

		SelectAllCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.SelectAll();
			return default;
		});

		DeleteCommand = Command.Factory.Create(() =>
		{
			webview.EditCommands.Delete();
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

		//PropertyChanged += OnPropertyChanged;
		webview.PropertyChanged += WebViewOnPropertyChanged;
	}

	[PropertyChanged(nameof(CurrentAddress))]
	private ValueTask OnCurrentAddressChangedAsync(string value)
	{
		Debug.WriteLine("OnAddressChanged: " + value);
		Address = value;
		return default;
	}

	private void WebViewOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
		Debug.WriteLine("Property Name: " + e.Property.Name);
		if (e.Property.Name == nameof(webview.Address))
		{
			//Debug.WriteLine("- Update Property");
			//Debug.WriteLine($" - {webview.CanGoBack} {webview.CanGoForward}");
			
			WebViewCanGoBack = webview.CanGoBack;
			WebViewCanGoForward = webview.CanGoForward;

			Debug.WriteLine("- Update Property");
			Debug.WriteLine($" - {WebViewCanGoBack} {WebViewCanGoForward}");
		}
	}

	private void WebView_Navigated(string url, string frameName)
	{
		Debug.WriteLine("WebView Navigated: " + url + " | " + frameName);
		//Debug.WriteLine("- Update Property");
		//Debug.WriteLine($" - {webview.CanGoBack} {webview.CanGoForward}");
		
		WebViewCanGoBack = webview.CanGoBack;
		WebViewCanGoForward = webview.CanGoForward;

		Debug.WriteLine("- Update Property");
		Debug.WriteLine($" - {WebViewCanGoBack} {WebViewCanGoForward}");
	}

	private void WebView_GoBack()
	{
		beforeAddress = webview.Address;
		webview.GoBack();
		//WebViewCanGoBack = webview.CanGoBack;
	}

	private void WebView_GoForward()
	{
		beforeAddress = Address;
		webview.GoForward();
		//WebViewCanGoForward = webview.CanGoForward;
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
