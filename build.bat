@echo off
REM Windows Multi-Monitor Display App - Build Script
REM このスクリプトでPythonスクリプトをexeに変換します

echo.
echo ========================================
echo Windows Monitor Display App Builder
echo ========================================
echo.

REM Python と pip の確認
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo エラー: Pythonがインストールされていません
    echo https://www.python.org からPythonをインストールしてください
    pause
    exit /b 1
)

echo ステップ 1: 依存パッケージをインストール中...
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo エラー: パッケージのインストールに失敗しました
    pause
    exit /b 1
)

echo.
echo ステップ 2: PyInstallerをインストール中...
pip install pyinstaller
if %errorlevel% neq 0 (
    echo エラー: PyInstallerのインストールに失敗しました
    pause
    exit /b 1
)

echo.
echo ステップ 3: exeにビルド中...
pyinstaller --onefile --windowed --icon=None monitor_display.py
if %errorlevel% neq 0 (
    echo エラー: ビルドに失敗しました
    pause
    exit /b 1
)

echo.
echo ========================================
echo ビルド完了！
echo ========================================
echo.
echo exeファイルは以下の場所に作成されました:
echo   dist\monitor_display.exe
echo.
echo このファイルをファイルサーバーに配置してください
echo ユーザーはこれをダブルクリックして実行できます
echo.
pause
