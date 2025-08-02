@echo off

echo Build Start
echo - Windows (x64) - Release
echo.

rem dotnet publish .\naget\SearchLight.csproj -c Release -r win-x64
rem dotnet publish -c Release

rem dotnet clean
rem dotnet build .\naget\naget.csproj -c Release -r win-x64


dotnet publish -c Release -r win-x64 /p:Platform=x64

call "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" ".\naget\Setup\naget_Setup_Windows-x64.iss"

echo.
echo Build Finish
echo.

pause
