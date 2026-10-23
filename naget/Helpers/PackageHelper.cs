using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace naget.Helpers;

/// <summary>
/// MSIX パッケージ環境に関するヘルパー
/// </summary>
public static class PackageHelper
{
	private const int ErrorSuccess = 0;
	private const int ErrorInsufficientBuffer = 122;
	private const int AppModelErrorNoPackage = 15700;

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, [Out] char[]? packageFullName);

	/// <summary>
	/// 現在のプロセスのパッケージ フル ネームを返す (非パッケージ環境では null)
	/// </summary>
	public static string? GetPackageFullName()
	{
		if (!OperatingSystem.IsWindows()) return null;

		int length = 0;
		int result = GetCurrentPackageFullName(ref length, null);
		if (result == AppModelErrorNoPackage) return null;
		if (result != ErrorInsufficientBuffer) return null;

		char[] buffer = new char[length];
		result = GetCurrentPackageFullName(ref length, buffer);
		return result == ErrorSuccess && length > 0 ? new string(buffer, 0, length - 1) : null;
	}

	/// <summary>
	/// アプリが MSIX パッケージとして実行されているかどうかを返す
	/// </summary>
	public static bool IsPackaged()
	{
		return GetPackageFullName() != null;
	}

	/// <summary>
	/// パッケージ ファミリー ネームを返す (非パッケージ環境では null)
	/// </summary>
	public static string? GetPackageFamilyName()
	{
		string? fullName = GetPackageFullName();
		if (fullName == null) return null;

		string[] parts = fullName.Split('_');
		return parts.Length >= 3 ? parts[0] + "_" + parts[^1] : null;
	}

	/// <summary>
	/// Microsoft Store のアプリ ページを開く
	/// </summary>
	public static void OpenStorePage()
	{
		string? familyName = GetPackageFamilyName();
		string uri = familyName != null
			? "ms-windows-store://pdp/?PFN=" + Uri.EscapeDataString(familyName)
			: "ms-windows-store://search/?query=" + Uri.EscapeDataString(App.ProductName);

		Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
	}
}
