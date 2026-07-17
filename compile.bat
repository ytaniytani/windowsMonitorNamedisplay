@echo off
REM Windows Multi-Monitor Display App - C# Compiler
REM This script compiles the C# source into a standalone .exe
REM No installation needed - uses built-in Windows C# compiler

echo.
echo ========================================
echo Windows Monitor Display App Compiler
echo ========================================
echo.

REM Find csc.exe (C# compiler - built-in to Windows)
setlocal enabledelayedexpansion
set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "!CSC_PATH!" (
    echo [ERROR] C# compiler not found.
    echo This should be built-in to Windows, but could not locate:
    echo   !CSC_PATH!
    pause
    exit /b 1
)

echo [Info] Found C# compiler at: !CSC_PATH!
echo.
echo [Step 1] Compiling C# source code...

"!CSC_PATH!" ^
  /target:winexe ^
  /out:MonitorDisplay.exe ^
  /r:System.dll ^
  /r:System.Drawing.dll ^
  /r:System.Windows.Forms.dll ^
  MonitorDisplay.cs

if %errorlevel% neq 0 (
    echo [ERROR] Compilation failed.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Compilation complete!
echo ========================================
echo.
echo The executable was created: MonitorDisplay.exe
echo.
echo Copy this file to your file server so users can run it.
echo Users need only Windows 10 or 11 - no additional installation required.
echo.
pause
