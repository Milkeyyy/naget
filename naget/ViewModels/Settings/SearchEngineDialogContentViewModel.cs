using Epoxy;

namespace naget.ViewModels.Settings;

[ViewModel]
public class SearchEngineDialogContentViewModel
{
	public string Name { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
}
