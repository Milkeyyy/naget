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
			PixelPoint? centerPos = HotKeyHelper.GetCenterScreen(App.MainWindow);
			if (centerPos is PixelPoint pos)
			{
				App.MainWindow.Position = pos;
			}
		}
	}

	public static void ShowAccessibilityPermissionWindow(bool userInitiated = true, bool dueToHookError = false)
	{
		if (App.AccessibilityPermissionWindow == null) return;

		if (App.AccessibilityPermissionWindow.DataContext is AccessibilityPermissionWindowViewModel vm)
		{
			if (dueToHookError) vm.SetHookError();
			if (userInitiated) vm.EnableLiveCheck();
			else vm.RefreshPermissionState();
		}

		if (!App.AccessibilityPermissionWindow.IsVisible) App.AccessibilityPermissionWindow.Show();
		ActivateWindow(App.AccessibilityPermissionWindow);
	}

	public static void ShowBrowser(string url)
	{
		if (App.BrowserWindow != null)
		{
			if (App.BrowserWindow.DataContext is BrowserWindowViewModel vm)
			{
				if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
				{
					vm.OpenNewTab(uri);
				}
				else
				{
					vm.OpenNewTab();
				}
			}
			App.BrowserWindow.Show();
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

	public static void ShowSettingsWindow()
	{
		if (App.SettingsWindow == null) return;

		if (!App.SettingsWindow.IsVisible) App.SettingsWindow.Show();
		ActivateWindow(App.SettingsWindow);
	}

	public static Window? GetSettingsWindow()
	{
		return App.SettingsWindow;
	}
}
