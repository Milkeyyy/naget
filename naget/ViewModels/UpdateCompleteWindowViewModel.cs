using Epoxy;
using naget.Assets.Locales;
using System.Text;

namespace naget.ViewModels;

[ViewModel]
public class UpdateCompleteWindowViewModel
{
	public static string WindowTitle => Resources.Updater_Dialog_Complete_Title + " - " + App.ProductName;
	public static string TitleText => Resources.Updater_Dialog_Complete_Title;
	private static CompositeFormat descriptionResource = CompositeFormat.Parse(Resources.Updater_Dialog_Complete_Description);
	public static string DescriptionText => string.Format(null, descriptionResource, App.ProductFullVersion);

	public Command CloseCommand { get; }

	public UpdateCompleteWindowViewModel()
	{
		CloseCommand = Command.Factory.Create(() =>
		{
			App.UpdateCompleteWindow.Hide();
			return default;
		});
	}
}
