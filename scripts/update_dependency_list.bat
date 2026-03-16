@echo off
nuget-license -i ..\naget.sln --output json > ..\naget\library.json
python update_dependency_list.py
pause