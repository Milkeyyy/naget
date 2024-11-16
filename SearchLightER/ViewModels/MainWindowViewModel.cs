using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SearchLight.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string ProductNameText { get; } = App.ProductName;
		public string ProductVersionText { get; } = App.ProductVersion;
	}
}
