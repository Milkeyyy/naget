using naget.Assets.Locales;
using SharpHook.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace naget.Models.Config.HotKey;

public class HotKeyGroup
{
	/// <summary>
	/// ホットキー固有のID
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// ホットキーの名前
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// キーの一覧
	/// </summary>
	public HashSet<KeyCode>? Keys { get; set; }

	/// <summary>
	/// アクションのクラス
	/// </summary>
	//[JsonIgnore]
	[JsonPropertyName("Action")]
	public HotKeyAction Action { get; private set; }

	[JsonIgnore]
	public HotKeyActionType ActionType
	{
		get { return Action.ActionType; }
		set { if (Action.ActionType != value) Action = new HotKeyAction(value); } // アクションのタイプが変更されたらアクションを再生成 (すでに同じアクションの場合は行わない)
	}

	[JsonIgnore]
	public Dictionary<string, string> ActionProperty => Action.Property;

	public HotKeyGroup(string name, HashSet<KeyCode>? keys = null)
	{
		Id = Guid.NewGuid().ToString();
		Name = name;
		Keys = keys;
		Action = new HotKeyAction(HotKeyActionType.None);
	}

	/// <summary>
	/// JSONデシリアライズ用コンストラクター
	/// </summary>
	/// <param name="Id"></param>
	/// <param name="Name"></param>
	/// <param name="Keys"></param>
	/// <param name="Action"></param>
	[JsonConstructor]
	public HotKeyGroup(string Id, string Name, HashSet<KeyCode> Keys, HotKeyAction Action)
	{
		this.Id = Id;
		this.Name = Name;
		this.Keys = Keys;
		this.Action = Action; //new(ActionDict.Id, ActionDict.Property);
	}

	public override string ToString()
	{
		if (Keys == null || Keys.Count == 0) return Resources.Settings_ShortcutKey_Preset_NotSet;
		return string.Join(" + ", Keys.Select(k => KeyCodeName.Get(k)));
	}
}

public static class KeyCodeName
{
	private static readonly Dictionary<KeyCode, List<string>> _keys = new() {
		{ KeyCode.VcUndefined, ["Undefined Key"] }, // Windows, Linux, macOS
		{ KeyCode.VcEscape, ["Escape"] },
		{ KeyCode.VcF1, ["F1"] },
		{ KeyCode.VcF2, ["F2"] },
		{ KeyCode.VcF3, ["F3"] },
		{ KeyCode.VcF4, ["F4"] },
		{ KeyCode.VcF5, ["F5"] },
		{ KeyCode.VcF6, ["F6"] },
		{ KeyCode.VcF7, ["F7"] },
		{ KeyCode.VcF8, ["F8"] },
		{ KeyCode.VcF9, ["F9"] },
		{ KeyCode.VcF10, ["F10"] },
		{ KeyCode.VcF11, ["F11"] },
		{ KeyCode.VcF12, ["F12"] },
		{ KeyCode.VcF13, ["F13"] },
		{ KeyCode.VcF14, ["F14"] },
		{ KeyCode.VcF15, ["F15"] },
		{ KeyCode.VcF16, ["F16"] },
		{ KeyCode.VcF17, ["F17"] },
		{ KeyCode.VcF18, ["F18"] },
		{ KeyCode.VcF19, ["F19"] },
		{ KeyCode.VcF20, ["F20"] },
		{ KeyCode.VcF21, ["F21", "F21", string.Empty] },
		{ KeyCode.VcF22, ["F22", "F22", string.Empty] },
		{ KeyCode.VcF23, ["F23", "F23", string.Empty] },
		{ KeyCode.VcF24, ["F24", "F24", string.Empty] },
		{ KeyCode.Vc0, ["0"] },
		{ KeyCode.Vc1, ["1"] },
		{ KeyCode.Vc2, ["2"] },
		{ KeyCode.Vc3, ["3"] },
		{ KeyCode.Vc4, ["4"] },
		{ KeyCode.Vc5, ["5"] },
		{ KeyCode.Vc6, ["6"] },
		{ KeyCode.Vc7, ["7"] },
		{ KeyCode.Vc8, ["8"] },
		{ KeyCode.Vc9, ["9"] },
		{ KeyCode.VcMinus, ["-"] },
		{ KeyCode.VcEquals, ["="] },
		{ KeyCode.VcBackspace, ["Backspace", "Backspace", "Delete"] },
		{ KeyCode.VcTab, ["Tab"] },
		{ KeyCode.VcCapsLock, ["Caps Lock"] },
		{ KeyCode.VcA, ["A"] },
		{ KeyCode.VcB, ["B"] },
		{ KeyCode.VcC, ["C"] },
		{ KeyCode.VcD, ["D"] },
		{ KeyCode.VcE, ["E"] },
		{ KeyCode.VcF, ["F"] },
		{ KeyCode.VcG, ["G"] },
		{ KeyCode.VcH, ["H"] },
		{ KeyCode.VcI, ["I"] },
		{ KeyCode.VcJ, ["J"] },
		{ KeyCode.VcK, ["K"] },
		{ KeyCode.VcL, ["L"] },
		{ KeyCode.VcM, ["M"] },
		{ KeyCode.VcN, ["N"] },
		{ KeyCode.VcO, ["O"] },
		{ KeyCode.VcP, ["P"] },
		{ KeyCode.VcQ, ["Q"] },
		{ KeyCode.VcR, ["R"] },
		{ KeyCode.VcS, ["S"] },
		{ KeyCode.VcT, ["T"] },
		{ KeyCode.VcU, ["U"] },
		{ KeyCode.VcV, ["V"] },
		{ KeyCode.VcW, ["W"] },
		{ KeyCode.VcX, ["X"] },
		{ KeyCode.VcY, ["Y"] },
		{ KeyCode.VcZ, ["Z"] },
		{ KeyCode.VcOpenBracket, ["["] },
		{ KeyCode.VcCloseBracket, ["]"] },
		{ KeyCode.VcBackslash, ["\\"] },
		{ KeyCode.VcSemicolon, [";"] },
		{ KeyCode.VcQuote, ["'"] },
		{ KeyCode.VcEnter, ["Enter"] },
		{ KeyCode.VcComma, [","] },
		{ KeyCode.VcPeriod, ["."] },
		{ KeyCode.VcSlash, ["/"] },
		{ KeyCode.VcSpace, ["Space"] },
		{ KeyCode.Vc102, ["<>/\\/|", "<>/\\/|", "§"] },
		{ KeyCode.VcMisc, ["OEM-specific Key"] },
		{ KeyCode.VcPrintScreen, ["Print Screen", "Print Screen", string.Empty] },
		{ KeyCode.VcScrollLock, ["Scroll Lock", "Scroll Lock", string.Empty] },
		{ KeyCode.VcPause, ["Pause", "Pause", string.Empty] },
		{ KeyCode.VcCancel, ["Cancel", "Cancel", string.Empty] },
		{ KeyCode.VcHelp, ["Help"] },
		{ KeyCode.VcInsert, ["Insert", "Insert", string.Empty] },
		{ KeyCode.VcDelete, ["Delete", "Delete", "Forward Delete"] },
		{ KeyCode.VcHome, ["Home"] },
		{ KeyCode.VcEnd, ["End"] },
		{ KeyCode.VcPageUp, ["Page Up"] },
		{ KeyCode.VcPageDown, ["Page Down"] },
		{ KeyCode.VcUp, ["↑"] },
		{ KeyCode.VcLeft, ["←"] },
		{ KeyCode.VcRight, ["→"] },
		{ KeyCode.VcDown, ["↓"] },
		{ KeyCode.VcNumLock, ["Num Lock", "Num Lock", string.Empty] },
		{ KeyCode.VcNumPadClear, ["Numpad Clear", string.Empty, "Numpad Clear"] },
		{ KeyCode.VcNumPadDivide, ["Numpad /"] },
		{ KeyCode.VcNumPadMultiply, ["Numpad *"] },
		{ KeyCode.VcNumPadSubtract, ["Numpad -"] },
		{ KeyCode.VcNumPadEquals, ["Numpad ="] },
		{ KeyCode.VcNumPadAdd, ["Numpad +"] },
		{ KeyCode.VcNumPadEnter, ["Numpad Enter"] },
		{ KeyCode.VcNumPadDecimal, ["Numpad Decimal"] },
		{ KeyCode.VcNumPadSeparator, ["Numpad Separator", "Numpad Separator", string.Empty] },
		{ KeyCode.VcNumPad0, ["Numpad 0"] },
		{ KeyCode.VcNumPad1, ["Numpad 1"] },
		{ KeyCode.VcNumPad2, ["Numpad 2"] },
		{ KeyCode.VcNumPad3, ["Numpad 3"] },
		{ KeyCode.VcNumPad4, ["Numpad 4"] },
		{ KeyCode.VcNumPad5, ["Numpad 5"] },
		{ KeyCode.VcNumPad6, ["Numpad 6"] },
		{ KeyCode.VcNumPad7, ["Numpad 7"] },
		{ KeyCode.VcNumPad8, ["Numpad 8"] },
		{ KeyCode.VcNumPad9, ["Numpad 9"] },
		{ KeyCode.VcLeftShift, ["Left Shift"] },
		{ KeyCode.VcRightShift, ["Right Shift"] },
		{ KeyCode.VcLeftControl, ["Left Control"] },
		{ KeyCode.VcRightControl, ["Right Control"] },
		{ KeyCode.VcLeftAlt, ["Left Alt", "Left Alt", "Option (Left)"] },
		{ KeyCode.VcRightAlt, ["Right Alt", "Right Alt", "Option (Right)"] },
		{ KeyCode.VcLeftMeta, ["Windows (Left)", "Super/Meta (Left)", "Command (Left)"] },
		{ KeyCode.VcRightMeta, ["Windows (Right)", "Super/Meta (Right)", "Command (Right)"] },
		{ KeyCode.VcContextMenu, ["Context Menu"] },
		{ KeyCode.VcFunction, [string.Empty, string.Empty, "Function"] },
		{ KeyCode.VcChangeInputSource, [string.Empty, string.Empty, "Change Input Source"] },
		{ KeyCode.VcPower, [string.Empty, "Power", "Power"] },
		{ KeyCode.VcSleep, ["Sleep", "Sleep", string.Empty] },
		{ KeyCode.VcMediaPlay, ["Play/Pause Media"] },
		{ KeyCode.VcMediaStop, ["Stop Media", "Stop Media", string.Empty] },
		{ KeyCode.VcMediaPrevious, ["Previous Media"] },
		{ KeyCode.VcMediaNext, ["Next Media"] },
		{ KeyCode.VcMediaSelect, ["Select Media", string.Empty, string.Empty] },
		{ KeyCode.VcMediaEject, [string.Empty, "Eject Media", "Eject Media"] },
		{ KeyCode.VcVolumeMute, ["Volume Mute"] },
		{ KeyCode.VcVolumeDown, ["Volume Down"] },
		{ KeyCode.VcVolumeUp, ["Volume Up"] },
		{ KeyCode.VcApp1, ["Launch App 1", "Launch App 1", string.Empty] },
		{ KeyCode.VcApp2, ["Launch App 2", "Launch App 2", string.Empty] },
		{ KeyCode.VcApp3, ["Launch App 3", "Launch App 3", string.Empty] },
		{ KeyCode.VcApp4, ["Launch App 4", "Launch App 4", string.Empty] },
		{ KeyCode.VcAppBrowser, [string.Empty, "Launch Browser", string.Empty] },
		{ KeyCode.VcAppCalculator, [string.Empty, "Launch Calculator", string.Empty] },
		{ KeyCode.VcAppMail, ["Launch Mail", string.Empty, string.Empty] },
		{ KeyCode.VcBrowserSearch, ["Browser Search", "Browser Search", string.Empty] },
		{ KeyCode.VcBrowserHome, ["Browser Home", "Browser Home", string.Empty] },
		{ KeyCode.VcBrowserBack, ["Browser Back", "Browser Back", string.Empty] },
		{ KeyCode.VcBrowserForward, ["Browser Forward", "Browser Forward", string.Empty] },
		{ KeyCode.VcBrowserStop, ["Browser Stop", string.Empty, string.Empty] },
		{ KeyCode.VcBrowserRefresh, ["Browser Refresh", "Browser Refresh", string.Empty] },
		{ KeyCode.VcBrowserFavorites, ["Browser Favorites", "Browser Favorites", string.Empty] },
		{ KeyCode.VcKatakanaHiragana, [string.Empty, "IME Katakana/Hiragana Toggle", string.Empty] },
		{ KeyCode.VcKatakana, [string.Empty, "IME Katakana Mode", string.Empty] },
		{ KeyCode.VcHiragana, [string.Empty, "IME Hiragana Mode", string.Empty] },
		{ KeyCode.VcKana, ["IME Kana Mode", string.Empty, "IME Kana Mode"] },
		{ KeyCode.VcKanji, ["IME Kanji Mode", string.Empty, string.Empty] },
		{ KeyCode.VcHangul, ["IME Hangul Mode", string.Empty, string.Empty] },
		{ KeyCode.VcJunja, ["IME Junja Mode", string.Empty, string.Empty] },
		{ KeyCode.VcFinal, ["IME Final Mode", string.Empty, string.Empty] },
		{ KeyCode.VcHanja, ["IME Hanja Mode", "IME Hanja Mode", string.Empty] },
		{ KeyCode.VcAccept, ["IME Accept", string.Empty, string.Empty] },
		{ KeyCode.VcConvert, ["IME Convert (Henkan)", "IME Convert (Henkan)", string.Empty] },
		{ KeyCode.VcNonConvert, ["IME Non-Convert (Muhenkan)", "IME Non-Convert (Muhenkan)", string.Empty] },
		{ KeyCode.VcImeOn, ["IME On", string.Empty, string.Empty] },
		{ KeyCode.VcImeOff, ["IME Off", string.Empty, string.Empty] },
		{ KeyCode.VcModeChange, ["IME Mode Change", "IME Mode Change", string.Empty] },
		{ KeyCode.VcProcess, ["IME Process", string.Empty, string.Empty] },
		{ KeyCode.VcAlphanumeric, [string.Empty, string.Empty, "IME Alphanumeric Mode (Eisu)"] },
		{ KeyCode.VcUnderscore, [string.Empty, "_", "_"] },
		{ KeyCode.VcYen, [string.Empty, "Yen", "Yen"] },
		{ KeyCode.VcJpComma, [string.Empty, "Comma (JP)", "Comma (JP)"] },
	};

	public static string Get(KeyCode key)
	{
		int i = 0;
		if (OperatingSystem.IsWindows()) { i = 0; }
		else if (OperatingSystem.IsLinux()) { i = 1; } // Linux では2つ目の文字列を使用
		else if (OperatingSystem.IsMacOS()) { i = 2; } // macOS では3つ目の文字列を使用
		List<string> n = _keys.GetValueOrDefault(key, []);
		if (n.Count == 0) return key.ToString(); // キーコードが登録されていない場合はキーコードの文字列を返す
		else if (n.Count == 1) return n[0]; // 1つだけ登録されている場合はその文字列を返す (共通)
		return n[i]; // それ以外はOSごとに適切な文字列を返す
	}
}
