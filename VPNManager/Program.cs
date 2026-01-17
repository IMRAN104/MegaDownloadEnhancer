using System;
using System.Windows.Forms;
using VPNManager.Forms;

namespace VPNManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Check for single instance
            bool createdNew;
            var mutex = new System.Threading.Mutex(true, "VPNManager_SingleInstance", out createdNew);

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
                Application.Run(new MainForm());
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
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
