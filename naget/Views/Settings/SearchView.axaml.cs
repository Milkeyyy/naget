using Avalonia.Controls;
using naget.ViewModels.Settings;

namespace naget.Views.Settings;

public partial class SearchView : UserControl
{
	public SearchView()
	{
		InitializeComponent();

		DataContext = new SearchViewModel();
	}
}