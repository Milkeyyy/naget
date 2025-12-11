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

rem 引数チェック: --skip-upload が含まれているかどうか
set SKIP_UPLOAD=0
:parse_args
if "%~1"=="" goto args_done
if /i "%~1"=="--skip-upload" set SKIP_UPLOAD=1
shift
goto parse_args
:args_done

rem Load env File
call ./scripts/load_env.bat ./Build_env.txt

echo.
echo naget - Build Start
echo.

cd %~dp0

echo Load Build Info
echo.
for /f "usebackq delims=" %%A in (`git rev-parse --short HEAD`) do set CommitHash=%%A
call ./build.cmd loadandsavebuildinfojson --releasechannel %ReleaseChannel% --releasenumber %CommitHash%
call winget install jqlang.jq
for /f "usebackq delims=" %%A in (`jq -r ".version" "./naget/build.json"`) do set AppVersion=%%A
for /f "usebackq delims=" %%A in (`jq -r ".full_version" "./naget/build.json"`) do set AppFullVersion=%%A
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

echo Build Installer and Upload - Velopack
echo.
call dotnet tool install -g vpk

pushd "%OutputDir%/%Runtime%"

echo Download Previous Release - Velopack
call vpk download s3 --bucket naget-update --endpoint "%R2_ENDPOINT%" --channel %VelopackChannel% -o ./Releases
if %ERRORLEVEL% neq 0 echo Warning: Failed to download previous release.

echo Build Installer - Velopack
call vpk pack -xy -u naget -v %AppFullVersion% -p ./Build -o ./Releases -e naget.exe --channel %VelopackChannel% --packAuthors Milkeyyy -i ..\..\Logo\naget.ico --noPortable

if %SKIP_UPLOAD%==1 (
    echo Upload SKIPPED
) else (
    echo Upload - Velopack
    call vpk upload -xy s3 --bucket naget-update --endpoint "%R2_ENDPOINT%" --channel %VelopackChannel% -o ./Releases
)

popd
echo.

echo.
echo Build Finished
echo.

pause
