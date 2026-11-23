using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace naget.Helpers;

[SupportedOSPlatform("windows")]
internal static class WebView2Interop
{
	public static void DisableStatusBar(NativeWebView webView)
	{
		if (webView.TryGetPlatformHandle() is not IWindowsWebView2PlatformHandle handle) return;
		if (handle.CoreWebView2 == IntPtr.Zero) return;

		ICoreWebView2? core = null;
		ICoreWebView2Settings? settings = null;
		try
		{
			core = (ICoreWebView2)Marshal.GetObjectForIUnknown(handle.CoreWebView2);
			settings = core.GetSettings();
			settings.SetIsStatusBarEnabled(false);
		}
		catch (Exception ex)
		{
			App.Logger.Debug("Failed to disable WebView2 status bar: " + ex.Message);
		}
		finally
		{
			if (settings is not null) Marshal.ReleaseComObject(settings);
			if (core is not null) Marshal.ReleaseComObject(core);
		}
	}
}

[ComImport]
[Guid("76ECEACB-0462-4D94-AC83-423A6793775E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICoreWebView2
{
	[return: MarshalAs(UnmanagedType.Interface)]
	ICoreWebView2Settings GetSettings();
}

[ComImport]
[Guid("E562E4F0-D7FA-43AC-8D71-C05150499F00")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICoreWebView2Settings
{
	[return: MarshalAs(UnmanagedType.Bool)]
	bool GetIsScriptEnabled();
	void SetIsScriptEnabled([MarshalAs(UnmanagedType.Bool)] bool value);

	[return: MarshalAs(UnmanagedType.Bool)]
	bool GetIsWebMessageEnabled();
	void SetIsWebMessageEnabled([MarshalAs(UnmanagedType.Bool)] bool value);

	[return: MarshalAs(UnmanagedType.Bool)]
	bool GetAreDefaultScriptDialogsEnabled();
	void SetAreDefaultScriptDialogsEnabled([MarshalAs(UnmanagedType.Bool)] bool value);

	[return: MarshalAs(UnmanagedType.Bool)]
	bool GetIsStatusBarEnabled();
	void SetIsStatusBarEnabled([MarshalAs(UnmanagedType.Bool)] bool value);
}
