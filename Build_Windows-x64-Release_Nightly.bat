@echo off

echo Build Start
echo - Windows (x64) - Release (Nighlty)
echo.

rem dotnet publish .\SearchLightER\SearchLight.csproj -c Release -r win-x64
rem dotnet publish -c Release

rem dotnet clean
rem dotnet build .\SearchLightER\naget.csproj -c Release -r win-x64


dotnet publish -c Release -r win-x64 /p:Platform=x64

call "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" ".\SearchLightER\Setup\naget_Setup_Nightly_Windows-x64.iss" /dMyAppReleaseChannel="nightly" /dMyAppReleaseNumber="0"

echo.
echo Build Finish
echo.

pause
