using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;
using Tmds.DBus.Protocol;
using WebViewControl;

namespace SearchLight.ViewModels;

public class BrowserWindowViewModel : ViewModelBase
{
	private WebView webview;

	private string address = string.Empty;
	private string currentAddress = string.Empty;
	private string beforeAddress = string.Empty;
	public bool webviewCanGoBack { get; private set; }
	public bool webviewCanGoForward { get; private set; }

	public BrowserWindowViewModel(WebView wb)
	{
		webview = wb;
		webview.Navigated += WebView_Navigated;
		//Address = CurrentAddress = "http://www.google.com/";

		NavigateCommand = ReactiveCommand.Create(() => {
			CurrentAddress = Address;
		});

		ShowDevToolsCommand = ReactiveCommand.Create(() => {
			webview.ShowDeveloperTools();
		});

		CutCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.Cut();
		});

		CopyCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.Copy();
		});

		PasteCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.Paste();
		});

		UndoCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.Undo();
		});

		RedoCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.Redo();
		});

		SelectAllCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.SelectAll();
		});

		DeleteCommand = ReactiveCommand.Create(() => {
			webview.EditCommands.Delete();
		});

		BackCommand = ReactiveCommand.Create(
			WebView_GoBack/*,
			this.WhenAnyValue(
				x => x.WebViewCanGoBack
			)*/
		);

		ForwardCommand = ReactiveCommand.Create(
			WebView_GoForward/*,
            this.WhenAnyValue(
				x => x.WebViewCanGoForward
			)*/
		);

		PropertyChanged += OnPropertyChanged;
		webview.PropertyChanged += WebViewOnPropertyChanged;
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(CurrentAddress))
		{
			Address = CurrentAddress;
		}
	}

	private void WebViewOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
		Debug.WriteLine("Property Name: " + e.Property.Name);
		if (e.Property.Name == nameof(webview.Address))
		{
			Debug.WriteLine("- Update Property");
			Debug.WriteLine($" - {webview.CanGoBack} {webview.CanGoForward}");
			
			webviewCanGoBack = webview.CanGoBack;
			webviewCanGoForward = webview.CanGoForward;
		}
	}

	private void WebView_Navigated(string url, string frameName)
    {
		Debug.WriteLine("WebView Navigated: " + url + " | " + frameName);
		Debug.WriteLine("- Update Property");
		Debug.WriteLine($" - {webview.CanGoBack} {webview.CanGoForward}");
		//WebViewCanGoBack = webview.CanGoBack;
		//WebViewCanGoForward = webview.CanGoForward;
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

	public string Address
	{
		get => address;
		set => this.RaiseAndSetIfChanged(ref address, value);
	}

	public string CurrentAddress
	{
		get => currentAddress;
		set => this.RaiseAndSetIfChanged(ref currentAddress, value);
	}

	public ReactiveCommand<Unit, Unit> NavigateCommand { get; }

	public ReactiveCommand<Unit, Unit> ShowDevToolsCommand { get; }

	public ReactiveCommand<Unit, Unit> CutCommand { get; }

	public ReactiveCommand<Unit, Unit> CopyCommand { get; }

	public ReactiveCommand<Unit, Unit> PasteCommand { get; }

	public ReactiveCommand<Unit, Unit> UndoCommand { get; }

	public ReactiveCommand<Unit, Unit> RedoCommand { get; }

	public ReactiveCommand<Unit, Unit> SelectAllCommand { get; }

	public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

	public ReactiveCommand<Unit, Unit> BackCommand { get; }

	public ReactiveCommand<Unit, Unit> ForwardCommand { get; }
}
