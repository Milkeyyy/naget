@echo off

rem ------------------------------
set RuntimeOs=win
set RuntimeArch=x64
set Runtime=%RuntimeOs%-%RuntimeArch%
set ReleaseChannel=nightly
set AppCastBaseUrl=https://pub-c3f507b079e846ba847ffc7c2c2fa43b.r2.dev/%ReleaseChannel%/%Runtime%
set OutputDir=./_Pack
rem ------------------------------

rem Load env File
call ./scripts/load_env.bat ./Build_env.txt

echo.
echo naget - Build Start

cd %~dp0

for /f "usebackq delims=" %%A in (`git rev-parse --short HEAD`) do set CommitHash=%%A

echo -         Runtime: %Runtime%
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

echo Build Installer
echo.
call ./build.cmd buildinstaller --runtime %Runtime% --releasechannel %ReleaseChannel% --releasenumber %CommitHash%
echo.

echo Generate App Cast
echo.
call dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator
call netsparkle-generate-appcast -n naget -u %AppCastBaseUrl% -o windows-%RuntimeArch% -a %OutputDir%/%Runtime% -b %OutputDir%/%Runtime% -e exe --output-file-name appcast_%ReleaseChannel%_%Runtime% --output-type json --channel %ReleaseChannel%
echo.

echo Install AWS CLI
echo.
call msiexec.exe /i https://awscli.amazonaws.com/AWSCLIV2.msi /passive
echo.

echo Upload App Installer
echo.
rem App Installer
call aws s3 cp "%OutputDir%/%Runtime%/naget_Setup_%Runtime%.exe" "s3://naget-update/%ReleaseChannel%/%Runtime%/naget_Setup_%Runtime%.exe" --endpoint-url "%R2_ENDPOINT%"
echo.

echo Upload App Cast
echo.
rem App Cast
call aws s3 cp "%OutputDir%/%Runtime%/appcast_%ReleaseChannel%_%Runtime%.json" "s3://naget-update/appcast/appcast_%ReleaseChannel%_%Runtime%.json" --endpoint-url "%R2_ENDPOINT%"
echo.
rem App Cast Signature
call aws s3 cp "%OutputDir%/%Runtime%/appcast_%ReleaseChannel%_%Runtime%.json.signature" "s3://naget-update/appcast/appcast_%ReleaseChannel%_%Runtime%.json.signature" --endpoint-url "%R2_ENDPOINT%"
echo.

echo.
echo Build Finished
echo.

pause
