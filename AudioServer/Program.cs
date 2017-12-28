using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioClient
{
    class Program
    {
        public const string EXENAME = "AudioClientBeta.exe";

        static void Main(string[] args)
        {
            try
            {
                //while (!File.Exists("a"))
                //{
                //    string a = "";
                //}
                string argument = string.Empty;
                //获取文件名无后缀
                string processName = Path.GetFileNameWithoutExtension(EXENAME);
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = false;
                psi.FileName = EXENAME;

                #region 判断程序是否运行，若运行Kill之
                Process[] MyProcesses = Process.GetProcesses();
                foreach (Process MyProcess in MyProcesses)
                {
                    if (MyProcess.ProcessName.CompareTo(processName) == 0)
                    {
                        MyProcess.Kill();
                    }
                }
                #endregion

                #region 判断是否传递了参数，若无参数不运行
                if (args.Count() >0)
                {
                    if (!string.IsNullOrEmpty(args[0]))
                    {
                        psi.Arguments = args[0];
                        Process.Start(psi);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
    }
}
