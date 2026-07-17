using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsMonitorDisplay
{
    // ---- DisplayConfig API (same API the Windows Settings app uses) ----

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_RATIONAL
    {
        public uint Numerator;
        public uint Denominator;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint outputTechnology;
        public uint rotation;
        public uint scaling;
        public DISPLAYCONFIG_RATIONAL refreshRate;
        public uint scanLineOrdering;
        public int targetAvailable; // BOOL
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_INFO
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_MODE_INFO
    {
        public uint infoType;
        public uint id;
        public LUID adapterId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] modeUnion; // union blob (largest member is 48 bytes)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public uint type;
        public uint size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string viewGdiDeviceName; // e.g. \\.\DISPLAY1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint flags;
        public uint outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint connectorInstance;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string monitorFriendlyDeviceName; // e.g. EV2455
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string monitorDevicePath;
    }

    static class NativeMethods
    {
        public const uint QDC_ONLY_ACTIVE_PATHS = 2;
        public const uint DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
        public const uint DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;

        [DllImport("user32.dll")]
        public static extern int GetDisplayConfigBufferSizes(uint flags,
            out uint numPathArrayElements, out uint numModeInfoArrayElements);

        [DllImport("user32.dll")]
        public static extern int QueryDisplayConfig(uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
            ref uint numModeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [DllImport("user32.dll")]
        public static extern int DisplayConfigGetDeviceInfo(
            ref DISPLAYCONFIG_SOURCE_DEVICE_NAME deviceName);

        [DllImport("user32.dll")]
        public static extern int DisplayConfigGetDeviceInfo(
            ref DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName);
    }

    static class Program
    {
        private static List<Form> _allForms = new List<Form>();
        private static Dictionary<string, string> _friendlyNames; // \\.\DISPLAY1 -> EV2455

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();

            var screens = Screen.AllScreens;

            if (screens.Length == 0)
            {
                MessageBox.Show("No monitors detected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < screens.Length; i++)
            {
                MonitorForm form = new MonitorForm(i, screens[i]);
                _allForms.Add(form);
                form.Show();
            }

            Application.Run();
        }

        public static void CloseAllForms()
        {
            foreach (var form in _allForms.ToList())
            {
                if (!form.IsDisposed)
                {
                    form.Close();
                }
            }
            Application.Exit();
        }

        // Builds a map from GDI device name (\\.\DISPLAY1) to the monitor's
        // friendly name (e.g. "EV2455"), using the same DisplayConfig API
        // that the Windows Settings app uses.
        private static Dictionary<string, string> BuildFriendlyNameMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                uint pathCount, modeCount;
                int err = NativeMethods.GetDisplayConfigBufferSizes(
                    NativeMethods.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != 0) return map;

                var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

                err = NativeMethods.QueryDisplayConfig(
                    NativeMethods.QDC_ONLY_ACTIVE_PATHS,
                    ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err != 0) return map;

                for (int i = 0; i < pathCount; i++)
                {
                    // Source: gives us the GDI name (\\.\DISPLAY1) that matches Screen.DeviceName
                    var source = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                    source.header.type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                    source.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME));
                    source.header.adapterId = paths[i].sourceInfo.adapterId;
                    source.header.id = paths[i].sourceInfo.id;
                    if (NativeMethods.DisplayConfigGetDeviceInfo(ref source) != 0) continue;

                    // Target: gives us the monitor's friendly name from EDID
                    var target = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                    target.header.type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                    target.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME));
                    target.header.adapterId = paths[i].targetInfo.adapterId;
                    target.header.id = paths[i].targetInfo.id;
                    if (NativeMethods.DisplayConfigGetDeviceInfo(ref target) != 0) continue;

                    string gdiName = (source.viewGdiDeviceName ?? "").TrimEnd('\0');
                    string friendly = (target.monitorFriendlyDeviceName ?? "").TrimEnd('\0').Trim();

                    if (gdiName.Length > 0 && friendly.Length > 0 && !map.ContainsKey(gdiName))
                    {
                        map[gdiName] = friendly;
                    }
                }
            }
            catch { }

            return map;
        }

        public static string GetMonitorName(Screen screen, int index)
        {
            if (_friendlyNames == null)
            {
                _friendlyNames = BuildFriendlyNameMap();
            }

            string deviceName = (screen.DeviceName ?? "").TrimEnd('\0');
            string friendly;
            if (_friendlyNames.TryGetValue(deviceName, out friendly))
            {
                return friendly;
            }

            return string.Format("Monitor {0}", index + 1);
        }
    }

    public class MonitorForm : Form
    {
        private int _monitorIndex;

        public MonitorForm(int index, Screen screen)
        {
            _monitorIndex = index;

            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(26, 26, 46); // #1a1a2e
            this.TopMost = false;
            this.ControlBox = false;

            // Move to specific monitor
            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.Bounds.Location;
            this.Size = screen.Bounds.Size;

            // Monitor index label
            Label labelIndex = new Label
            {
                Text = string.Format("Monitor {0}", _monitorIndex + 1),
                Font = new Font("Arial", 120, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255), // #00d4ff
                BackColor = Color.FromArgb(26, 26, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = screen.Bounds.Width,
                Height = 200,
                Top = 100
            };

            // Monitor friendly name label (e.g. "EV2455")
            string monitorName = Program.GetMonitorName(screen, _monitorIndex);
            Label labelName = new Label
            {
                Text = monitorName,
                Font = new Font("Arial", 60),
                ForeColor = Color.FromArgb(0, 255, 136), // #00ff88
                BackColor = Color.FromArgb(26, 26, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = screen.Bounds.Width,
                Height = 150,
                Top = 320
            };

            // Resolution label
            string resolution = string.Format("{0} × {1}", screen.Bounds.Width, screen.Bounds.Height);
            Label labelResolution = new Label
            {
                Text = resolution,
                Font = new Font("Arial", 40),
                ForeColor = Color.FromArgb(255, 170, 0), // #ffaa00
                BackColor = Color.FromArgb(26, 26, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = screen.Bounds.Width,
                Height = 100,
                Top = 500
            };

            // Close button (top-right corner)
            Button closeButton = new Button
            {
                Text = "×",
                Font = new Font("Arial", 40, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.FromArgb(26, 26, 46),
                FlatStyle = FlatStyle.Flat,
                Width = 80,
                Height = 80,
                Top = 10,
                Left = screen.Bounds.Width - 90,
                TabStop = false
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += delegate { Program.CloseAllForms(); };

            this.Controls.Add(labelIndex);
            this.Controls.Add(labelName);
            this.Controls.Add(labelResolution);
            this.Controls.Add(closeButton);

            // ESC closes everything. CancelButton makes ESC trigger the close
            // button even when a child control has focus; KeyDown is a backup.
            this.CancelButton = closeButton;
            this.KeyPreview = true;
            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Program.CloseAllForms();
                    e.Handled = true;
                }
            };
        }
    }
}
