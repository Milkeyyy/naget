using System;
using System.Runtime.InteropServices;

namespace naget.Common;

public static class Utils
{
	public static int ConvertToInt(string str, int def = 0)
	{
		return int.TryParse(str, out int result) ? result : (def != 0 ? def : 0);
	}
}

public static class WindowUtils
{
	// P/Invoke declarations
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetForegroundWindow(IntPtr hWnd);
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsIconic(IntPtr hWnd);
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	[DllImport("user32.dll")]
	private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
	private const int SW_RESTORE = 9;
	private const int INPUT_MOUSE = 0;
	[StructLayout(LayoutKind.Sequential)]
	private struct INPUT
	{
		public int type;
		public MOUSEINPUT mi;
	}
	[StructLayout(LayoutKind.Sequential)]
	private struct MOUSEINPUT
	{
		public int dx;
		public int dy;
		public uint mouseData;
		public uint dwFlags;
		public uint time;
		public IntPtr dwExtraInfo;
	}
	public struct HWND
	{
		public IntPtr Handle { get; set; }
		public static implicit operator IntPtr(HWND hwnd) => hwnd.Handle;
		public static implicit operator HWND(IntPtr handle) => new HWND { Handle = handle };
	}

	/// <summary>
	/// 指定したウィンドウを最前面に表示する
	/// </summary>
	/// <param name="hwnd">ウィンドウのハンドル</param>
	public static void ForceToForeground(HWND hwnd)
	{
		// Restore if minimized
		if (IsIconic(hwnd))
		{
			ShowWindow(hwnd, SW_RESTORE);
		}

		INPUT[] inputs = new INPUT[1];
		inputs[0].type = INPUT_MOUSE;

		// 構造体の初期化が必要な場合は、inputs[0].mi = new MOUSEINPUT(); などを追加してください
		_ = SendInput(1, inputs, Marshal.SizeOf<INPUT>());

		// Now SetForegroundWindow will work
		SetForegroundWindow(hwnd);
	}
}
