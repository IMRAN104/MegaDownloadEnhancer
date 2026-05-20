using System;
using System.Threading;
using System.Windows.Forms;
using VPNManager.Forms;

namespace VPNManager
{
    static class Program
    {
        private static Mutex? _mutex;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            _mutex = new Mutex(true, "VPNManager_SingleInstance", out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "VPN Manager is already running!",
                    "Already Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                var mainForm = new MainForm();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }
    }
}
