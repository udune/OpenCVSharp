using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCVSharp
{
    internal static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                // Register OpenCV native DLL path (dll/x86 or dll/x64) in the search paths
                string is64 = IntPtr.Size == 8 ? "x64" : "x86";
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll", is64);
                if (Directory.Exists(dllPath))
                {
                    SetDllDirectory(dllPath);
                }

                Application.Run(new ChapterSelectorForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
