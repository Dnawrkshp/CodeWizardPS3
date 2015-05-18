using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CodeWizardPS3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Check if instance is already running and if there are multiple environment arguments
            if (Environment.GetCommandLineArgs().Length > 2 &&
                System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                    System.Diagnostics.Process.GetCurrentProcess().Kill();

            string[] dependencies = new string[] { "FastColoredTextBox.dll", "JacksonSoft.CustomTabControl.dll" };
            string depNoExist = "";
            foreach (string dep in dependencies)
            {
                if (!File.Exists(Application.StartupPath + "\\" + dep))
                    depNoExist += "\"" + dep + "\" does not exist!\n";
            }
            if (depNoExist != "")
            {
                DialogResult dialogResult = MessageBox.Show(depNoExist);
                Environment.Exit(0);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
