@echo off
echo Building QuickLauncher...
echo.

REM Restore NuGet packages
echo [1/3] Restoring packages...
dotnet restore

echo.
echo [2/3] Building project...
dotnet build --configuration Release --no-restore

echo.
echo [3/3] Publishing executable...
dotnet publish --configuration Release --no-build --output .\bin\Release\Publish

echo.
echo ====================================
echo Build Complete!
echo ====================================
echo.
echo Executable location: .\bin\Release\Publish\QuickLauncher.exe
echo.
pause
