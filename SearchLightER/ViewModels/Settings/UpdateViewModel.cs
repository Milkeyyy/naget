using Avalonia.Controls;
using Epoxy;
using naget.Models.Config;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace naget.ViewModels.Settings;

[ViewModel]
public class UpdateViewModel
{
	public Well<UserControl> ViewWell { get; } = Well.Factory.Create<UserControl>();

	private bool ViewIsLoaded;

	public Command CheckUpdateCommand { get; }

	public UpdateViewModel()
	{
		// ビューがロードされた時の処理
		ViewWell.Add(Control.LoadedEvent, () =>
		{
			Debug.WriteLine("UpdateView Loaded");
			ViewIsLoaded = true;
			return default;
		});

		CheckUpdateCommand = Command.Factory.Create(async () =>
		{
			Debug.WriteLine("Execute Check Update Command");
			await App.ManualUpdateCheck();
		});
	}
}
