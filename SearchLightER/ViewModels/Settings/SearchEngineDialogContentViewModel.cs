using Epoxy;
using naget.Models.Config;

namespace naget.ViewModels.Settings;

[ViewModel]
public class SearchEngineDialogContentViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
}
