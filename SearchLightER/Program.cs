using Avalonia;
using Avalonia.Media;
using System;

namespace naget
{
	internal sealed class Program
	{
		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		[STAThread]
		public static void Main(string[] args)
		{
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.With(new FontManagerOptions
				{
					DefaultFamilyName = "avares://naget/Assets/Fonts#Noto Sans JP",
					FontFallbacks = [
						new FontFallback { FontFamily = new FontFamily("avares://naget/Assets/Fonts#Noto Sans JP") }
					]
				})
				.LogToTrace();
	}
}
