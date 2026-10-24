using System;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS
using Microsoft.Win32;
using Velopack.Locators;
using Windows.ApplicationModel;
#endif

namespace naget.Helpers;

public static class StartupManager
{
#if WINDOWS
	private const string WindowsValueName = "naget";
	private const string WindowsRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
	private const string WindowsApprovedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
	private const string PackagedTaskId = "nagetStartup";
#else
	private const string MacLaunchAgentLabel = "com.milkeyyy.naget";
	private const string MacLaunchAgentFileName = MacLaunchAgentLabel + ".plist";
#endif

#if !WINDOWS
	private static string MacLaunchAgentPath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		"Library",
		"LaunchAgents",
		MacLaunchAgentFileName);
#endif

	public static async Task<bool> GetEnabledAsync()
	{
		try
		{
#if WINDOWS
			return await GetWindowsEnabledAsync();
#else
			return IsMacLaunchAgentEnabled();
#endif
		}
		catch (Exception ex)
		{
			App.Logger?.Warn("Failed to read startup setting: " + ex.Message);
			return false;
		}
	}

	public static async Task<bool> SetEnabledAsync(bool enabled)
	{
		try
		{
#if WINDOWS
			return await SetWindowsEnabledAsync(enabled);
#else
			return SetMacLaunchAgent(enabled);
#endif
		}
		catch (Exception ex)
		{
			App.Logger?.Error(ex);
			return false;
		}
	}

#if WINDOWS
	public static void RemoveWindowsStartupEntry()
	{
		try
		{
			using RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(WindowsRunKey, writable: true);
			runKey?.DeleteValue(WindowsValueName, throwOnMissingValue: false);

			using RegistryKey? approvedKey = Registry.CurrentUser.OpenSubKey(WindowsApprovedKey, writable: true);
			approvedKey?.DeleteValue(WindowsValueName, throwOnMissingValue: false);
		}
		catch
		{
		}
	}

	private static Task<bool> GetWindowsEnabledAsync()
	{
		if (PackageHelper.IsPackaged())
		{
			return GetPackagedEnabledAsync();
		}

		return Task.FromResult(IsWindowsRunEntryEnabled());
	}

	private static Task<bool> SetWindowsEnabledAsync(bool enabled)
	{
		if (PackageHelper.IsPackaged())
		{
			return SetPackagedEnabledAsync(enabled);
		}

		return Task.FromResult(SetWindowsRunEntry(enabled));
	}

	private static async Task<bool> GetPackagedEnabledAsync()
	{
		StartupTask task = await StartupTask.GetAsync(PackagedTaskId);
		return task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
	}

	private static async Task<bool> SetPackagedEnabledAsync(bool enabled)
	{
		StartupTask task = await StartupTask.GetAsync(PackagedTaskId);
		if (!enabled)
		{
			if (task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy)
			{
				task.Disable();
			}

			return true;
		}

		StartupTaskState state = await task.RequestEnableAsync();
		return state is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
	}

	private static bool IsWindowsRunEntryEnabled()
	{
		using RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(WindowsRunKey);
		if (runKey?.GetValue(WindowsValueName) is not string value || string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		using RegistryKey? approvedKey = Registry.CurrentUser.OpenSubKey(WindowsApprovedKey);
		if (approvedKey?.GetValue(WindowsValueName) is byte[] data && data.Length > 0)
		{
			return (data[0] & 1) == 0;
		}

		return true;
	}

	private static bool SetWindowsRunEntry(bool enabled)
	{
		if (enabled)
		{
			string? executablePath = GetStartupExecutablePath();
			if (string.IsNullOrEmpty(executablePath))
			{
				return false;
			}

			using RegistryKey? runKey = Registry.CurrentUser.CreateSubKey(WindowsRunKey, writable: true);
			if (runKey == null)
			{
				return false;
			}

			runKey.SetValue(WindowsValueName, "\"" + executablePath + "\"", RegistryValueKind.String);
			RemoveWindowsStartupApprovedEntry();
			return true;
		}

		using (RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(WindowsRunKey, writable: true))
		{
			runKey?.DeleteValue(WindowsValueName, throwOnMissingValue: false);
		}

		RemoveWindowsStartupApprovedEntry();
		return true;
	}

	private static void RemoveWindowsStartupApprovedEntry()
	{
		using RegistryKey? approvedKey = Registry.CurrentUser.OpenSubKey(WindowsApprovedKey, writable: true);
		approvedKey?.DeleteValue(WindowsValueName, throwOnMissingValue: false);
	}
#else
	private static bool IsMacLaunchAgentEnabled()
	{
		return File.Exists(MacLaunchAgentPath);
	}

	private static bool SetMacLaunchAgent(bool enabled)
	{
		if (!enabled)
		{
			if (File.Exists(MacLaunchAgentPath))
			{
				File.Delete(MacLaunchAgentPath);
			}

			return true;
		}

		string? executablePath = GetStartupExecutablePath();
		if (string.IsNullOrEmpty(executablePath))
		{
			return false;
		}

		string directory = Path.GetDirectoryName(MacLaunchAgentPath)!;
		Directory.CreateDirectory(directory);

		string escapedPath = SecurityElement.Escape(executablePath) ?? string.Empty;
		string plist = $"""
			<?xml version="1.0" encoding="UTF-8"?>
			<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
			<plist version="1.0">
			<dict>
				<key>Label</key>
				<string>{MacLaunchAgentLabel}</string>
				<key>ProgramArguments</key>
				<array>
					<string>{escapedPath}</string>
				</array>
				<key>RunAtLoad</key>
				<true/>
				<key>KeepAlive</key>
				<false/>
				<key>ProcessType</key>
				<string>Interactive</string>
			</dict>
			</plist>
			""";

		string tempPath = MacLaunchAgentPath + ".tmp";
		File.WriteAllText(tempPath, plist, new UTF8Encoding(false));
		File.Move(tempPath, MacLaunchAgentPath, true);
		return true;
	}
#endif

	private static string? GetStartupExecutablePath()
	{
#if WINDOWS
		if (VelopackLocator.IsCurrentSet && VelopackLocator.Current.CurrentlyInstalledVersion != null)
		{
			string? root = VelopackLocator.Current.RootAppDir;
			string? relativePath = VelopackLocator.Current.ThisExeRelativePath;
			if (!string.IsNullOrEmpty(root) && !string.IsNullOrEmpty(relativePath))
			{
				string path = Path.Combine(root, relativePath);
				if (File.Exists(path))
				{
					return path;
				}
			}
		}
#endif

		return Environment.ProcessPath;
	}
}
