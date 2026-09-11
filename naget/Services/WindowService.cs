using Avalonia;
using Avalonia.Controls;
using naget.Helpers;
using naget.Common;
using naget.ViewModels;
using System;

namespace naget.Services;

public static class WindowService
{
	public static void ShowMainWindow()
	{
		if (App.MainWindow != null)
		{
			App.MainWindow.Show();
			ActivateWindow(App.MainWindow);
		}
	}

	public static void HideMainWindow()
	{
		App.MainWindow?.Hide();
	}

	public static void CenterMainWindow()
	{
		if (App.MainWindow != null)
		{
			// マウスカーソルがあるディスプレイの中央位置を取得
			var centerPos = HotKeyHelper.GetCenterScreen(App.MainWindow);
			// 中央位置を取得できた場合はウィンドウの位置をその中央へ移動する
			App.MainWindow.Position = centerPos ?? new(0, 0);
		}
	}

	public static void ShowBrowser(string url)
	{
		if (App.BrowserWindow != null)
		{
			App.BrowserWindow.Show();
			if (App.BrowserWindow.DataContext is BrowserWindowViewModel vm)
			{
				if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
				{
					vm.CurrentSource = uri;
				}
				vm.Address = url;
			}
		}
	}

	private static void ActivateWindow(Window window)
	{
		var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
		if (handle != IntPtr.Zero)
		{
			WindowUtils.ForceToForeground(handle);
		}
		window.Activate();
		window.Focus();
	}

	public static void TriggerSearch(string engineId)
	{
		if (App.MainWindow?.DataContext is MainWindowViewModel vm)
		{
			vm.Search(engineId);
		}
	}

	public static Window? GetSettingsWindow()
	{
		return App.SettingsWindow;
	}
}
