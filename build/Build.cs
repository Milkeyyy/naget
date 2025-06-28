using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.InnoSetup;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

class Build : NukeBuild
{
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode

	public static int Main() => Execute<Build>(x => x.Compile);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	//readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
	readonly Configuration Configuration = Configuration.Release;
	[Parameter]
	string ReleaseChannel = null;
	[Parameter]
	string ReleaseNumber = null;
	[Parameter]
	string Runtime = null;
	[Parameter]
	string Platform => Runtime.Split("-")[1];

	readonly string ProjectFile = RootDirectory / "SearchLightER" / "naget.csproj";

	private Dictionary<string, string> GetBuildInfo()
	{
		var d = JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(RootDirectory / "SearchLightER" / "build.json"),
			new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				WriteIndented = true,
			}
		);

		if (ReleaseChannel != null)  { d["release_channel"] = ReleaseChannel; }
		if (ReleaseNumber != null) { d["release_number"] = ReleaseNumber; }

		int rn;
		string st = ".";
		// リリース番号が数字でない場合は . ではなく + で区切る
		try { rn = int.Parse(d["release_number"]); }
		catch { rn = -1; }
		if (rn == -1) st = "+";
		d["full_version"] = $"{d["version"]}-{d["release_channel"]}{st}{d["release_number"]}";
		
		return d;
	}

	Target Clean => _ => _
		.Before(Restore)
		.Executes(() =>
		{
		});

	Target Restore => _ => _
		.Executes(() =>
		{
			var buildInfo = GetBuildInfo();

			DotNetTasks.DotNetRestore(_ => _
				.SetProjectFile(ProjectFile)
				.SetRuntime(Runtime)
				.SetPlatform(Platform)
				.SetVersion(buildInfo["full_version"])
				.SetInformationalVersion(buildInfo["full_version"])
				.SetFileVersion(buildInfo["version"])
				.SetAssemblyVersion(buildInfo["version"])
			);
		});

	Target Compile => _ => _
		.DependsOn(Restore)
		.Executes(() =>
		{
			var buildInfo = GetBuildInfo();

			//DotNetTasks.DotNetBuild(_ => _
			//	.SetProjectFile(ProjectFile)
			//	.SetConfiguration(Configuration)
			//	.SetRuntime(Runtime)
			//	.SetPlatform(Platform)
			//	.SetVersion(buildInfo["version"])
			//	.SetFileVersion(buildInfo["version"])
			//	.SetAssemblyVersion(buildInfo["version"])
			//	.SetInformationalVersion(buildInfo["version"])
			//	.EnableNoRestore()
			//);
			DotNetTasks.DotNetPublish(_ => _
				.SetProject(ProjectFile)
				.SetConfiguration(Configuration)
				.SetRuntime(Runtime)
				.SetPlatform(Platform)
				.SetVersion(buildInfo["full_version"])
				.SetInformationalVersion(buildInfo["full_version"])
				.SetFileVersion(buildInfo["version"])
				.SetAssemblyVersion(buildInfo["version"])
				.EnableNoRestore()
			);
		});

	Target BuildInstaller => _ => _
		.Executes(() =>
		{
			AbsolutePath output = RootDirectory / "_Pack" / Runtime;

			var SetupArch = "x64compatible";
			if (Runtime == "win-arm64") SetupArch = "arm64";

			var buildInfo = GetBuildInfo();

			InnoSetupTasks.InnoSetup(c => c
				.SetKeyValueDefinition("MyAppVersion", buildInfo["version"])
				.SetKeyValueDefinition("MyAppReleaseChannel", buildInfo["release_channel"])
				.SetKeyValueDefinition("MyAppReleaseNumber", buildInfo["release_number"])
				.SetKeyValueDefinition("MyArch", SetupArch)
				.SetKeyValueDefinition("MyPlatformArch", Platform)
				.SetKeyValueDefinition("MyPlatform", Runtime)
				.SetOutputBaseFilename($"naget_Setup_{Runtime}")
				.SetOutputDir(output)
				.SetScriptFile(RootDirectory / "SearchLightER" / "Setup" / $"naget_Setup_{buildInfo["release_channel"]}.iss")
			);
		});

	Target BundleApp => _ => _
		.Executes(() =>
		{
			var buildInfo = GetBuildInfo();

			AbsolutePath directory = RootDirectory / "SearchLightER";
			AbsolutePath output = RootDirectory / "_Pack" / Runtime;

			DotNetTasks.DotNetRestore(_ => _
				.SetProjectFile(ProjectFile)
				.SetRuntime(Runtime)
				.SetPlatform(Platform)
				.SetVersion(buildInfo["full_version"])
				.SetInformationalVersion(buildInfo["full_version"])
				.SetFileVersion(buildInfo["version"])
				.SetAssemblyVersion(buildInfo["version"])
			);
			
			DotNetTasks.DotNetMSBuild(s => s
				.SetProcessWorkingDirectory(directory)
				.SetTargets("BundleApp")
				.SetConfiguration(Configuration)
				.SetVersion(buildInfo["full_version"])
				.SetInformationalVersion(buildInfo["full_version"])
				.SetFileVersion(buildInfo["version"])
				.SetAssemblyVersion(buildInfo["version"])
				.SetPlatform(Platform)
				.SetProperty("PublishDir", output)
				.SetProperty("CFBundleVersion", buildInfo["version"])
				.SetProperty("CFBundleShortVersionString", buildInfo["full_version"])
				.SetProperty("RuntimeIdentifier", Runtime)
				.SetProperty("UseAppHost", true)
				.SetProperty("SelfContained", false));
		});
}
