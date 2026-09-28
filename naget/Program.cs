using Avalonia;
using Avalonia.Media;
using System;
using Velopack;

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
			VelopackApp.Build().Run();

			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				// フォントの設定
				.With(new FontManagerOptions
				{
					FontFallbacks = CreateCjkFontFallbacks()
				})
				.With(new MacOSPlatformOptions()
				{
					// (macOS) Dock に表示しない
					ShowInDock = false,
				})
#if DEBUG
				.WithDeveloperTools()
#endif
				.LogToTrace();

		private static FontFallback[] CreateCjkFontFallbacks()
		{
			UnicodeRange cjkRange = UnicodeRange.Parse("U+3000-30FF, U+3400-4DBF, U+4E00-9FFF, U+F900-FAFF, U+FF00-FFEF");

			if (OperatingSystem.IsWindows())
			{
				return [
					new FontFallback
					{
						FontFamily = new FontFamily("Yu Gothic UI"),
						UnicodeRange = cjkRange
					}
				];
			}

			if (OperatingSystem.IsMacOS())
			{
				return [
					new FontFallback
					{
						FontFamily = new FontFamily("Hiragino Sans"),
						UnicodeRange = cjkRange
					}
				];
			}

			return [];
		}
	}
}
