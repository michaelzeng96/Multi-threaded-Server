using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;

            //mutex used to ensure only one instance of app is running at a time. held by the os
            Mutex m = new Mutex(true, "Client", out createdNew);

            if(!createdNew)
            {
                MessageBox.Show("Only one client can be running at a time. Bye");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            //keep mutex reference alive until the normal temrination of the program
            GC.KeepAlive(m);
        }
    }
}
