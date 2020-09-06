using System;
using System.Windows.Forms;

namespace LAUNCHER_FANBOT
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            //Press F
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Menu());
        }
    }
}
