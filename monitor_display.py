#!/usr/bin/env python3
"""
Windows Multi-Monitor Display App
各モニタにそれぞれのID番号と名前を大きく表示
"""

import tkinter as tk
from screeninfo import get_monitors
import sys
import threading

def get_monitor_name_from_windows(index):
    """Windows APIからモニタ名を取得"""
    try:
        import winreg
        reg = winreg.ConnectRegistry(None, winreg.HKEY_LOCAL_MACHINE)
        key = winreg.OpenKey(reg, r'SYSTEM\CurrentControlSet\Enum\DISPLAY')

        for i in range(winreg.QueryInfoKey(key)[0]):
            subkey_name = winreg.EnumKey(key, i)
            subkey = winreg.OpenKey(key, subkey_name)
            try:
                value = winreg.QueryValueEx(subkey, 'FriendlyName')
                if value:
                    return value[0]
            except:
                pass
            winreg.CloseKey(subkey)
        winreg.CloseKey(key)
    except:
        pass
    return f"Monitor {index + 1}"

def create_monitor_window(monitor_index, monitor):
    """モニタごとにウィンドウを作成・表示"""
    root = tk.Tk()
    root.attributes('-fullscreen', True)
    root.configure(bg='#1a1a2e')

    monitor_name = get_monitor_name_from_windows(monitor_index)

    frame = tk.Frame(root, bg='#1a1a2e')
    frame.pack(expand=True, fill=tk.BOTH)

    label_id = tk.Label(
        frame,
        text=f"Monitor {monitor_index + 1}",
        font=('Arial', 120, 'bold'),
        fg='#00d4ff',
        bg='#1a1a2e'
    )
    label_id.pack(pady=50)

    label_name = tk.Label(
        frame,
        text=monitor_name,
        font=('Arial', 60),
        fg='#00ff88',
        bg='#1a1a2e'
    )
    label_name.pack(pady=30)

    label_resolution = tk.Label(
        frame,
        text=f"{monitor.width} × {monitor.height}",
        font=('Arial', 40),
        fg='#ffaa00',
        bg='#1a1a2e'
    )
    label_resolution.pack(pady=20)

    # ESCキーで閉じる
    root.bind('<Escape>', lambda e: root.destroy())

    # ウィンドウをそのモニタに配置
    root.geometry(f"+{monitor.x}+{monitor.y}")

    root.mainloop()

def main():
    monitors = get_monitors()

    if not monitors:
        print("モニタが検出されません")
        sys.exit(1)

    print(f"{len(monitors)}個のモニタを検出しました")

    threads = []
    for i, monitor in enumerate(monitors):
        print(f"Monitor {i}: {monitor}")
        thread = threading.Thread(target=create_monitor_window, args=(i, monitor))
        thread.daemon = False
        thread.start()
        threads.append(thread)

    for thread in threads:
        thread.join()

if __name__ == '__main__':
    main()
