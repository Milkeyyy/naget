using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace naget.Helpers;

public static class MacDockHelper
{
	private const int NSApplicationActivationPolicyRegular = 0;
	private const int NSApplicationActivationPolicyAccessory = 1;

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr objc_getClass(string name);

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr sel_registerName(string name);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	private static extern void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

	private static readonly HashSet<Window> trackedWindows = [];

	private static bool? isShown;

	public static void Initialize()
	{
		if (!OperatingSystem.IsMacOS()) return;

		Window.WindowOpenedEvent.AddClassHandler(typeof(Window), (sender, _) =>
		{
			if (sender is Window window) Track(window);
		});

		Window.WindowClosedEvent.AddClassHandler(typeof(Window), (sender, _) =>
		{
			if (sender is Window window) Untrack(window);
		});
	}

	private static void Track(Window window)
	{
		if (!trackedWindows.Add(window)) return;
		window.PropertyChanged += OnWindowPropertyChanged;
	}

	private static void Untrack(Window window)
	{
		if (!trackedWindows.Remove(window)) return;
		window.PropertyChanged -= OnWindowPropertyChanged;
	}

	private static void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (e.Property == Visual.IsVisibleProperty) Update();
	}

	private static void Update()
	{
		bool visible = false;

		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			foreach (Window window in desktop.Windows)
			{
				if (!ReferenceEquals(window, App.MainWindow) && window.IsVisible)
				{
					visible = true;
					break;
				}
			}
		}

		SetShowInDock(visible);
	}

	private static void SetShowInDock(bool show)
	{
		if (isShown == show) return;

		try
		{
			IntPtr nsApplication = objc_msgSend_IntPtr(objc_getClass("NSApplication"), sel_registerName("sharedApplication"));
			if (nsApplication == IntPtr.Zero) return;

			int policy = show ? NSApplicationActivationPolicyRegular : NSApplicationActivationPolicyAccessory;
			objc_msgSend_void_IntPtr(nsApplication, sel_registerName("setActivationPolicy:"), policy);
			isShown = show;
		}
		catch (Exception ex)
		{
			App.Logger.Warn("Failed to change the macOS activation policy: " + ex.Message);
		}
	}
}
