using System;
using System.Windows.Forms;

namespace CxHashDecoder
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoaderForm loader = new();
            loader.ShowDialog();
            if (loader.Isinitialized)
            {
                Application.Run(loader.MainForm);
            }
        }
    }
}
