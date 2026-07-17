@echo off
REM Windows Multi-Monitor Display App - Build Script
REM This script builds the Python script into a standalone .exe

echo.
echo ========================================
echo Windows Monitor Display App Builder
echo ========================================
echo.

REM Check for Python
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Python is not installed or not on PATH.
    echo Please install Python from https://www.python.org
    echo Make sure to check "Add Python to PATH" during installation.
    pause
    exit /b 1
)

echo [Step 1] Installing dependencies...
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo [ERROR] Failed to install dependencies.
    pause
    exit /b 1
)

echo.
echo [Step 2] Installing PyInstaller...
pip install pyinstaller
if %errorlevel% neq 0 (
    echo [ERROR] Failed to install PyInstaller.
    pause
    exit /b 1
)

echo.
echo [Step 3] Building the .exe ...
pyinstaller --onefile --windowed monitor_display.py
if %errorlevel% neq 0 (
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build complete!
echo ========================================
echo.
echo The .exe was created at:
echo   dist\monitor_display.exe
echo.
echo Copy this file to your file server so users can run it.
echo.
pause
