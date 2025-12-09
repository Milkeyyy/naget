@echo off

rem ------------------------------
set RuntimeOs=win
set RuntimeArch=x64
set Runtime=%RuntimeOs%-%RuntimeArch%
set ReleaseChannel=nightly
set AppCastBaseUrl=https://nagetupd.milkeyyy.com/%ReleaseChannel%/%Runtime%
set OutputDir=./_Pack
set VelopackChannel=%RuntimeOs%-%RuntimeArch%-%ReleaseChannel%
rem ------------------------------

rem Load env File
call ./scripts/load_env.bat ./Build_env.txt

echo.
echo naget - Build Start
echo.

cd %~dp0

echo Load Build Info
echo.
call ./build.cmd loadandsavebuildinfojson --releasechannel %ReleaseChannel% --releasenumber %CommitHash%
call winget install jqlang.jq
for /f "usebackq delims=" %%A in (`jq -r ".version" "./naget/build.json"`) do set AppVersion=%%A
for /f "usebackq delims=" %%A in (`jq -r ".full_version" "./naget/build.json"`) do set AppFullVersion=%%A
for /f "usebackq delims=" %%A in (`git rev-parse --short HEAD`) do set CommitHash=%%A
echo.

echo -         Runtime: %Runtime%
echo -         Version: %AppVersion%
echo - Release Channel: %ReleaseChannel%
echo -     Commit Hash: %CommitHash%
echo.

echo Cleanup Output Directory
echo.
if exist "%OutputDir%/%Runtime%" (
    rd /s /q "%OutputDir%/%Runtime%"
)
echo.

echo Create Output Directory
echo.
mkdir "%OutputDir%"
mkdir "%OutputDir%/%Runtime%"
echo.

echo Compile
echo.
call ./build.cmd --runtime %Runtime% --releasechannel %ReleaseChannel% --releasenumber %CommitHash%
echo.

echo Build Installer and Upload (Velopack)
echo.
call dotnet tool install -g vpk

pushd "%OutputDir%/%Runtime%"

echo Download Previous Release (Velopack)
call vpk download s3 --bucket naget-update --endpoint "%R2_ENDPOINT%" --channel %VelopackChannel%
if %ERRORLEVEL% neq 0 echo Warning: Failed to download previous release.

rem Clean up existing version to avoid overwrite prompt
if exist "Releases" (
    echo Cleaning up existing version %AppFullVersion%...
    del /f /q "Releases\naget-%AppFullVersion%-win-%RuntimeArch%-full.nupkg" 2>nul
    del /f /q "Releases\naget-%AppFullVersion%-win-%RuntimeArch%-delta.nupkg" 2>nul
    del /f /q "Releases\naget-%AppFullVersion%-win-%RuntimeArch%-Setup.exe" 2>nul
    
    if exist "Releases\releases.%VelopackChannel%.json" (
        jq "del(.Assets[] | select(.Version == \"%AppFullVersion%\"))" "Releases\releases.%VelopackChannel%.json" > "Releases\releases.tmp.json"
        move /y "Releases\releases.tmp.json" "Releases\releases.%VelopackChannel%.json"
    )
)

echo Build Installer (Velopack)
call vpk pack -u naget -v %AppFullVersion% -p . -e naget.exe --channel %VelopackChannel% --packAuthors Milkeyyy -i ..\..\Logo\naget.ico --noPortable

echo Upload (Velopack)
call vpk upload s3 --bucket naget-update --endpoint "%R2_ENDPOINT%" --channel %VelopackChannel%

popd
echo.

echo.
echo Build Finished
echo.

pause
