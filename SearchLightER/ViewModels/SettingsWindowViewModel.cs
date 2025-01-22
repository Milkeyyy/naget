using Epoxy;
using SearchLight.Assets.Locales;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SearchLight.ViewModels;

[ViewModel]
public class SettingsWindowViewModel
{
	public string RegisteredKeysText { get; private set; }
	public string KeyRegisterButtonText { get; private set; }
	public bool KeyRegistrationMode { get; private set; }
	public Command KeyRegisterCommand { get; }
	public Command ExitCommand { get; }

	public SettingsWindowViewModel()
	{
		RegisteredKeysText = string.Empty;
		KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;

		KeyRegisterCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute KeyRegisterCommand");
			Debug.WriteLine("- KeyRegistrationMode: " + KeyRegistrationMode);
			if (KeyRegistrationMode)
			{
				// キー登録モードを終了する
				var result = App.HotKeyManager.EndKeyRegistration();
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			}
			else
			{
				// キー登録モードに入る
				KeyRegistrationMode = true;
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Done;
				var result = await App.HotKeyManager.StartKeyRegistrationAsync();
				if (result != string.Empty && result != null)
				{
					var keys = App.HotKeyManager.GetHotKeyGroupFromKey(result);
					RegisteredKeysText = string.Join(", ", keys);
				}
				KeyRegistrationMode = false;
				KeyRegisterButtonText = Resources.Settings_ShortcutKey_RegisterKeys_Register;
			}
		});

		ExitCommand = Command.Factory.Create(() =>
		{
			Environment.Exit(0);
			return default;
		});
	}

	[PropertyChanged(nameof(KeyRegistrationMode))]
	private ValueTask KeyRegistrationModeChanged()
	{
		Debug.WriteLine("KeyRegistrationMode Changed: " + KeyRegistrationMode);
		KeyRegisterButtonText = KeyRegistrationMode ? Resources.Settings_ShortcutKey_RegisterKeys_Done : Resources.Settings_ShortcutKey_RegisterKeys_Register;
		return default;
	}
}
