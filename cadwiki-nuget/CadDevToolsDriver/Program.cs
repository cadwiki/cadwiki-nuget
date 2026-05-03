
using System;
using System.Windows;

namespace CadDevToolsDriver
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var dependencies = Launcher.GetDependencies();
            Window Window = new cadwiki.CadDevTools.MainWindow(dependencies);

            var app = new Application();
            app.Run(Window);
        }
    }
}