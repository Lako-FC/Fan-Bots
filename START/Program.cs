using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace START
{
    internal class Program
    {
        /// <summary>
        /// Вызов MessageBox.Show(...) с готовыми параметрами показа ошибки.
        /// </summary>
        public static void MSB_Error(string text) => MessageBox.Show(text, "Fan-Bot's", MessageBoxButtons.OK, MessageBoxIcon.Error);
        /// <summary>
        /// Список exe-файлов (launcher.exe, wb.exe).
        /// </summary>
        public static readonly string[] files = { "launcher.exe", "wb.exe" };
        /// <summary>
        /// Основная папка Fan-Bot's: bin\
        /// </summary>
        public static readonly string dir = @"bin\";
        /// <summary>
        /// Файл первого запуска, если его нет, то запуск происходит без "Окна первого запуска" (соглашение и т.д).
        /// </summary>
        public static string one_message = $"{dir}one_message";

        [STAThread]
        private static void Main()
        {
            if (!File.Exists("NO_CHECK.fanbot") && 
                Process.GetProcessesByName("Game").Length > 0 || 
                Process.GetProcessesByName("GameCenter").Length > 0)
                Environment.Exit(0);

            if (File.Exists(one_message))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new One_Message());
            }
            else new One_Message().Start();
        }
    }
}