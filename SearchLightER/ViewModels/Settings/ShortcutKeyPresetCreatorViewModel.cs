using Epoxy;
using naget.Models.Config;

namespace naget.ViewModels.Settings;

[ViewModel]
public class ShortcutKeyPresetCreatorViewModel
{
	public string PresetName { get; set; }

	public ShortcutKeyPresetCreatorViewModel()
	{
		PresetName = string.Empty;
	}
}
