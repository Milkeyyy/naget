using Epoxy;

namespace naget.ViewModels;

[ViewModel]
public class AboutWindowViewModel
{
	public static string WindowTitle => App.ProductName;
	public static string TitleText => App.ProductName;
	public static string VersionText => "Version " + App.ProductFullVersion;
}
