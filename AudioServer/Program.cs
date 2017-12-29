using Anthony.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AudioClient
{
    class Program
    {
        public const string EXENAME = "AudioClientBeta.exe";
        private static ARLogger logger = ARLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            try
            {
                if (args.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(args[0]))
                    {
                        logger.Info(string.Format("前台调用AudioClient，传递了参数：{0}", args[0]));
                    }
                }
                else
                {
                    logger.Info(string.Format("前台调用AudioClient，无参数"));
                }
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
                        logger.Info("发现{0}正在运行，关闭之。", EXENAME);
                        MyProcess.Kill();
                    }
                }
                #endregion

                #region 判断是否传递了参数，若无参数不运行
                if (args.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(args[0]))
                    {
                        logger.Info(string.Format("前台调用AudioClient，执行启动指挥端AudioClientBeta工作程序。"));
                        psi.Arguments = args[0];
                        Process.Start(psi);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error("指挥端启动失败.{0}", ex.Message);
                Console.ReadKey();
            }
        }
    }
}
