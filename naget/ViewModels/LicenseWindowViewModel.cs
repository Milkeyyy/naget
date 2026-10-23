using Epoxy;
using naget.Assets.Locales;
using naget.Models;
using System.Collections.Generic;

namespace naget.ViewModels;

[ViewModel]
public class LicenseWindowViewModel
{
	public static string WindowTitle => Resources.About_Licenses_Title + " - " + App.ProductName;
	public static string TitleText => Resources.About_Licenses_Title;
	public static string DescriptionText => Resources.About_Licenses_Description;
	public IReadOnlyList<LibraryInfo> Libraries { get; } = App.Libraries;
}
