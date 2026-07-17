# Windows Multi-Monitor Display App

Windowsマルチモニタ環境で、各モニタにそのIDと名前を大きく表示するアプリケーション。

## 機能

- 接続されているすべてのモニタを検出
- 各モニタに専用ウィンドウで表示:
  - **モニタ番号** (1, 2, 3...)
  - **モニタ名** (OS設定から取得)
  - **解像度** (横×縦ピクセル数)
- ESCキーで終了

## 使用方法

### 方法1: Pythonから直接実行（開発環境）

```bash
pip install -r requirements.txt
python monitor_display.py
```

### 方法2: exeとしてビルド・配布（推奨）

#### ビルド手順

1. `build.bat`をダブルクリック
2. ビルドが完了すると `dist/monitor_display.exe` が作成されます

#### ファイルサーバーへの配置

1. `dist/monitor_display.exe` をファイルサーバーにアップロード
2. ユーザーに共有フォルダのパスを通知
3. ユーザーが`monitor_display.exe`をダブルクリックして実行

**初回実行時の注意:**
- Windows Defenderが「不明な発行元」と表示する場合がありますが、「実行」をクリックすれば起動します
- IT部門が実行を許可する必要がある場合は事前に確認してください

## ファイル構成

```
windows-monitor-app/
├── monitor_display.py      # メインアプリケーション
├── requirements.txt         # Python依存パッケージ
├── build.bat               # exeビルドスクリプト
└── README.md               # このファイル
```

## トラブルシューティング

### "Pythonが見つかりません" エラー
→ https://www.python.org からPythonをインストールしてください

### モニタが検出されない
→ Windowsのディスプレイ設定を確認してください

### exeが実行できない
→ 会社のセキュリティポリシーを確認してください

## 技術仕様

- **言語**: Python 3.7+
- **GUI**: tkinter (標準ライブラリ)
- **モニタ検出**: screeninfo
- **ビルド**: PyInstaller

## ライセンス

MIT License
