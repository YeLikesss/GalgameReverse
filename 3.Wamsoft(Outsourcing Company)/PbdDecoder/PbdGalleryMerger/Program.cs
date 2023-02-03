using System;
using System.Collections;
using System.Windows.Forms;

namespace PbdGalleryMerger
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            Application.Run(new MainForm());
        }
    }
}