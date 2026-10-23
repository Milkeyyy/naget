using System;

namespace naget.Models;

public class LibraryInfo
{
	public string PackageId { get; set; } = string.Empty;
	public string PackageVersion { get; set; } = string.Empty;
	public string? PackageProjectUrl { get; set; }
	public string? Copyright { get; set; }
	public string? Authors { get; set; }
	public string? License { get; set; }
	public Uri? LicenseUrl { get; set; }
}
