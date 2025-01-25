using Epoxy;
using SearchLight.Models.Config;

namespace SearchLight.ViewModels.Settings;

[ViewModel]
public class ShortcutKeyPresetCreatorViewModel
{
	public string PresetName { get; set; }

	public ShortcutKeyPresetCreatorViewModel()
	{
		PresetName = string.Empty;
	}
}
