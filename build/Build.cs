using System;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

		string commitHash = Repository?.Commit?[..7] ?? "unknown";
		d["commit_hash"] = commitHash;

		string buildDate = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
		d["build_date"] = buildDate;
		d["release_number"] = buildDate;

		if (ReleaseChannel != null) { d["release_channel"] = ReleaseChannel; }
		if (ReleaseNumber != null) { d["release_number"] = ReleaseNumber; }

		//int rn;
		string st = ".";
		// リリース番号が数字でない場合は . ではなく + で区切る
		// try { rn = int.Parse(d["release_number"]); }
		// catch { rn = -1; }
		// if (rn == -1) st = "+";
		d["full_version"] = $"{d["version"]}-{d["release_channel"]}{st}{d["release_number"]}";

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

	private static string RunProcess(string fileName, string arguments)
	{
		using Process process = new()
		{
			StartInfo = new ProcessStartInfo(fileName, arguments)
			{
				WorkingDirectory = RootDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				StandardOutputEncoding = Encoding.UTF8,
				StandardErrorEncoding = Encoding.UTF8,
				UseShellExecute = false,
			}
		};

		process.Start();
		string output = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
		{
			throw new InvalidOperationException(error.Trim());
		}

		return output;
	}

	private static void WriteLibraryList()
	{
		AbsolutePath libraryFile = ProjectFolder / "library.json";
		AbsolutePath customFile = RootDirectory / "scripts" / "custom.json";

		try
		{
			RunProcess("dotnet", "tool restore");
			string output = RunProcess("dotnet", $"tool run nuget-license -i \"{ProjectFile}\" --output json");
			List<LibraryInfo> libraries = JsonSerializer.Deserialize<List<LibraryInfo>>(output) ?? [];
			Dictionary<string, LibraryInfo> custom = JsonSerializer.Deserialize<Dictionary<string, LibraryInfo>>(File.ReadAllText(customFile)) ?? [];

			List<LibraryInfo> merged = [];
			foreach (LibraryInfo library in libraries)
			{
				if (custom.Remove(library.PackageId, out LibraryInfo customLibrary))
				{
					if (customLibrary != null) merged.Add(customLibrary);
				}
				else
				{
					merged.Add(library);
				}
			}
			merged.AddRange(custom.Values.OfType<LibraryInfo>());
			merged.Sort((a, b) => string.Compare(a.PackageId, b.PackageId, StringComparison.Ordinal));

			File.WriteAllText(libraryFile, JsonSerializer.Serialize(merged, new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				WriteIndented = true,
			}));
			Serilog.Log.Information($"library.json updated ({merged.Count} libraries)");
		}
		catch (Exception ex)
		{
			Serilog.Log.Warning($"Failed to update library.json: {ex.Message}");
		}
	}

	Target UpdateLibraryList => _ => _
		.DependsOn(Restore)
		.Executes(WriteLibraryList);

	Target Compile => _ => _
		.DependsOn(Restore)
		.DependsOn(UpdateLibraryList)
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

class LibraryInfo
{
	public string PackageId { get; set; }
	public string PackageVersion { get; set; }
	public string PackageProjectUrl { get; set; }
	public string Copyright { get; set; }
	public string Authors { get; set; }
	public string License { get; set; }
	public string LicenseUrl { get; set; }
}
