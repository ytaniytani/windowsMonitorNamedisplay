using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsMonitorDisplay
{
    // Windows API for getting display device info
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    static class Program
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        private static List<Form> _allForms = new List<Form>();

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

            Console.WriteLine(string.Format("Detected {0} monitor(s)", screens.Length));

            for (int i = 0; i < screens.Length; i++)
            {
                int index = i;
                Screen screen = screens[i];

                MonitorForm form = new MonitorForm(index, screen);
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

        public static string GetMonitorName(int index)
        {
            DISPLAY_DEVICE device = new DISPLAY_DEVICE();
            device.cb = (uint)Marshal.SizeOf(device);

            try
            {
                if (EnumDisplayDevices(null, (uint)index, ref device, 0))
                {
                    if (!string.IsNullOrEmpty(device.DeviceString))
                    {
                        return device.DeviceString.Trim();
                    }
                }
            }
            catch { }

            return string.Format("Monitor {0}", index + 1);
        }
    }

    public class MonitorForm : Form
    {
        private int _monitorIndex;
        private Screen _screen;

        public MonitorForm(int index, Screen screen)
        {
            _monitorIndex = index;
            _screen = screen;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46); // #1a1a2e
            this.TopMost = false;
            this.ControlBox = false;

            // Move to specific monitor
            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.Bounds.Location;
            this.Size = screen.Bounds.Size;

            // Monitor index label (top-left area)
            Label labelIndex = new Label
            {
                Text = string.Format("Monitor {0}", _monitorIndex + 1),
                Font = new Font("Arial", 120, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255), // #00d4ff
                BackColor = Color.FromArgb(26, 26, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.None,
                AutoSize = false,
                Width = screen.Bounds.Width,
                Height = 200,
                Top = 100
            };

            // Monitor name label (middle)
            string monitorName = Program.GetMonitorName(_monitorIndex);
            Label labelName = new Label
            {
                Text = monitorName,
                Font = new Font("Arial", 60),
                ForeColor = Color.FromArgb(0, 255, 136), // #00ff88
                BackColor = Color.FromArgb(26, 26, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.None,
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
                Dock = DockStyle.None,
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
            closeButton.Click += (s, e) => Program.CloseAllForms();

            this.Controls.Add(labelIndex);
            this.Controls.Add(labelName);
            this.Controls.Add(labelResolution);
            this.Controls.Add(closeButton);

            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Program.CloseAllForms();
                    e.Handled = true;
                }
            };

            Console.WriteLine(string.Format("Monitor {0}: {1} - {2}", _monitorIndex, screen.Bounds, monitorName));
        }
    }
}
