using Epoxy;
using naget.Assets.Locales;

namespace naget.ViewModels;

[ViewModel]
public class SettingsWindowViewModel
{
	public static string WindowTitle { get { return Resources.Window_Settings + " - " + App.ProductName; } }
}
