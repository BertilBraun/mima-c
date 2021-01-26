@echo off
dotnet publish -c Release -o release

rem Copy Folder out of Git directory
rmdir /Q /S ../release
for %%a in ("release\\\*") do move /y "%%~fa" ../release
for /d %%a in ("release\\\*") do move /y "%%~fa" ../release
rmdir /Q /S release

pause