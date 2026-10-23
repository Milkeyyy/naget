param(
	[string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $root "naget"
$packageDir = Join-Path $root "naget.Package"
$buildInfoPath = Join-Path $projectDir "build.json"
$appxManifestPath = Join-Path $packageDir "Package.appxmanifest"
$solutionPath = Join-Path $root "naget.sln"
$wapprojPath = Join-Path $packageDir "naget.Package.wapproj"

# バージョンを build.json から取得する
$buildInfoOriginal = [System.IO.File]::ReadAllText($buildInfoPath)
$buildInfo = $buildInfoOriginal | ConvertFrom-Json
$version = $buildInfo.version
$parts = $version.Split(".")
if ($parts.Length -eq 3) {
	$appxVersion = "$version.0"
}
elseif ($parts.Length -eq 4) {
	$appxVersion = $version
}
else {
	throw "build.json の version が不正です: $version"
}

# マニフェストのバージョンを更新する
$appxManifest = [System.IO.File]::ReadAllText($appxManifestPath)
$appxManifest = $appxManifest -replace '(<Identity[^>]*\sVersion=")[^"]*(")', ('${1}' + $appxVersion + '${2}')
[System.IO.File]::WriteAllText($appxManifestPath, $appxManifest, (New-Object System.Text.UTF8Encoding($false)))

# MSBuild.exe を探す (wapproj は dotnet CLI ではビルドできない)
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = $null
if (Test-Path $vswhere) {
	$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($msbuild)) {
	$msbuild = "msbuild"
}

try {
	# パッケージに埋め込む build.json を Store 向けにする (ビルド後に元へ戻す)
	$buildInfo.release_channel = "store"
	$buildInfo.release_number = "0"
	$buildInfo | Add-Member -NotePropertyName "full_version" -NotePropertyValue "$version-store.0" -Force
	[System.IO.File]::WriteAllText($buildInfoPath, ($buildInfo | ConvertTo-Json), (New-Object System.Text.UTF8Encoding($false)))

	Write-Host "Restoring..." -ForegroundColor Cyan
	& $msbuild $solutionPath /t:Restore /p:Configuration=$Configuration /p:Platform=x64 /nologo
	if ($LASTEXITCODE -ne 0) { throw "Restore に失敗しました ($LASTEXITCODE)" }

	Write-Host "Building $appxVersion..." -ForegroundColor Cyan
	& $msbuild $wapprojPath `
		/p:Configuration=$Configuration `
		/p:Platform=x64 `
		/p:UapAppxPackageBuildMode=StoreUpload `
		/p:AppxBundle=Always `
		/p:AppxBundlePlatforms="x64|arm64" `
		/nologo
	if ($LASTEXITCODE -ne 0) { throw "パッケージのビルドに失敗しました ($LASTEXITCODE)" }
}
finally {
	[System.IO.File]::WriteAllText($buildInfoPath, $buildInfoOriginal, (New-Object System.Text.UTF8Encoding($false)))
}

$upload = Get-ChildItem (Join-Path $packageDir "AppPackages") -Recurse -Filter *.msixupload |
	Sort-Object LastWriteTime -Descending |
	Select-Object -First 1
if ($null -eq $upload) { throw ".msixupload が見つかりませんでした" }

Write-Host "完了: $($upload.FullName)" -ForegroundColor Green
