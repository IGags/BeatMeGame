using System;
using System.Windows.Forms;

namespace BeatMeGame
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var mainWindow = new MainWindow();
            Application.Run(mainWindow);
        }
    }
}
