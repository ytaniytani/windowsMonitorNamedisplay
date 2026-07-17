using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsMonitorDisplay
{
    static class Program
    {
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

            List<System.Threading.Thread> threads = new List<System.Threading.Thread>();

            for (int i = 0; i < screens.Length; i++)
            {
                int index = i;
                Screen screen = screens[i];

                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    MonitorForm form = new MonitorForm(index, screen);
                    Application.Run(form);
                });

                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        static Dictionary<int, string> GetMonitorFriendlyNames()
        {
            var result = new Dictionary<int, string>();

            try
            {
                var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Enum\DISPLAY");

                if (regKey != null)
                {
                    int index = 0;
                    foreach (var subKeyName in regKey.GetSubKeyNames())
                    {
                        var subKey = regKey.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            var friendlyName = subKey.GetValue("FriendlyName");
                            if (friendlyName != null)
                            {
                                result[index] = friendlyName.ToString();
                            }
                            else
                            {
                                result[index] = string.Format("Monitor {0}", index + 1);
                            }
                            index++;
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        public static string GetMonitorName(int index)
        {
            var names = GetMonitorFriendlyNames();
            if (names.ContainsKey(index))
                return names[index];
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

            // Move to specific monitor
            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.Bounds.Location;
            this.Size = screen.Bounds.Size;

            // Create controls
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

            this.Controls.Add(labelIndex);
            this.Controls.Add(labelName);
            this.Controls.Add(labelResolution);

            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                    e.Handled = true;
                }
            };

            Console.WriteLine(string.Format("Monitor {0}: {1} - {2}", _monitorIndex, screen.Bounds, monitorName));
        }
    }
}
