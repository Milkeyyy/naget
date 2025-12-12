using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
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

	[GitRepository] readonly GitRepository Repository;

	readonly static AbsolutePath ProjectFolder = RootDirectory / "naget";
	readonly static AbsolutePath ProjectFile = ProjectFolder / "naget.csproj";

	private Dictionary<string, string> LoadAndSaveBuildInfo()
	{
		var d = JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(ProjectFolder / "build.json"),
			new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				WriteIndented = true,
			}
		);

		if (ReleaseChannel != null) { d["release_channel"] = ReleaseChannel; }
		if (ReleaseNumber != null) { d["release_number"] = ReleaseNumber; }

		int rn;
		string st = ".";
		// リリース番号が数字でない場合は . ではなく + で区切る
		try { rn = int.Parse(d["release_number"]); }
		catch { rn = -1; }
		if (rn == -1) st = "+";
		d["full_version"] = $"{d["version"]}-{d["release_channel"]}{st}{d["release_number"]}";

		string commit_hash = Repository.Commit.Substring(0, 7);
		d["commit_hash"] = commit_hash;

		// 書き換えたビルド情報を上書き保存する
		File.WriteAllText(ProjectFolder / "build.json", JsonSerializer.Serialize(d, options: new JsonSerializerOptions() { WriteIndented = true }));

		return d;
	}

	Target LoadAndSaveBuildInfoJson => _ => _
		.Executes(() =>
		{
			LoadAndSaveBuildInfo();
		});

	Target Clean => _ => _
		.Before(Restore)
		.Executes(() =>
		{
		});

	Target Restore => _ => _
		.Executes(() =>
		{
			var buildInfo = LoadAndSaveBuildInfo();

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
			var buildInfo = LoadAndSaveBuildInfo();
			AbsolutePath output = RootDirectory / "_Pack" / Runtime / "Build";

			DotNetTasks.DotNetPublish(_ => _
				.SetProject(ProjectFile)
				.SetConfiguration(Configuration)
				.SetRuntime(Runtime)
				.SetPlatform(Platform)
				.SetVersion(buildInfo["full_version"])
				.SetInformationalVersion(buildInfo["full_version"])
				.SetFileVersion(buildInfo["version"])
				.SetAssemblyVersion(buildInfo["version"])
				.SetOutput(output)
				.DisableSelfContained()
				.EnablePublishTrimmed()
				.EnableNoRestore()
			);
		});

	Target BundleApp => _ => _
		.DependsOn(Restore)
		.Executes(() =>
		{
			var buildInfo = LoadAndSaveBuildInfo();

			AbsolutePath directory = ProjectFolder;
			AbsolutePath output = RootDirectory / "_Pack" / Runtime / "Build";

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
				.SetProperty("CFBundleVersion", buildInfo["full_version"])
				.SetProperty("CFBundleShortVersionString", buildInfo["version"])
				.SetProperty("CFBundleIconFile", RootDirectory / "Logo" / "naget.icns")
				.SetProperty("RuntimeIdentifier", Runtime)
				.SetProperty("UseAppHost", true)
				.SetProperty("SelfContained", false)
				.EnablePublishTrimmed());
		});

	Target BuildPkg => _ => _
		.DependsOn(BundleApp)
		.Executes(() =>
		{

		});


}
