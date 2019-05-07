using System;
using System.Windows.Forms;

namespace ImageViewer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] argv)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (argv.Length > 0)
                Application.Run(new MainForm(argv[0]));
            else
                Application.Run(new MainForm(null));
        }
    }
}
