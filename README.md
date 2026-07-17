# Windows Multi-Monitor Display App

Displays each monitor's ID number and friendly name in large text across multiple monitors.

## Features

- Detects all connected monitors
- Displays on each monitor:
  - Monitor number (1, 2, 3...)
  - Monitor friendly name (e.g., "DELL U2720Q")
  - Resolution (width × height in pixels)
- Press ESC to exit
- **No installation required** - works on any Windows 10/11 system

## How to Use

### Step 1: Compile

Double-click `compile.bat`. The script will:
- Locate the built-in C# compiler (no download needed)
- Compile `MonitorDisplay.cs` into `MonitorDisplay.exe`

### Step 2: Run

Double-click `MonitorDisplay.exe` to display monitor info on all connected screens.

### Step 3: Deploy

Copy `MonitorDisplay.exe` to your file server. Users can download and run it directly - no software installation required.

## Requirements

- Windows 10 or Windows 11
- .NET Framework 4.5+ (built-in to Windows)
- That's it!

## Files

```
├── MonitorDisplay.cs       # C# source code
├── compile.bat             # Build script (uses built-in Windows compiler)
├── MonitorDisplay.exe      # Executable (generated after running compile.bat)
└── README.md               # This file
```

## Troubleshooting

### "C# compiler not found" error
This should not happen on Windows 10/11. The compiler is at:
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
```
If missing, .NET Framework may need repair. Run Windows Update.

### Monitor name shows as "Monitor 1", "Monitor 2", etc.
Windows could not retrieve friendly names from the registry. This is normal on some systems - the monitor is still detected and displayed.

### The program won't start
Ensure you're on Windows 10 or 11. Older Windows versions may have issues.

## Technical Details

- **Language**: C# (.NET Framework 4.5+)
- **Compilation**: Built-in Windows C# compiler (`csc.exe`)
- **GUI**: Windows Forms
- **Monitor detection**: `System.Windows.Forms.Screen` class
- **Monitor names**: Windows Registry (`HKLM\SYSTEM\CurrentControlSet\Enum\DISPLAY`)

## License

MIT License
