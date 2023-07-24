using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdgeGateway
{
    internal static class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            RestService _rest;

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //var type = Environment.Is64BitProcess ?
            //    "x64" : "x86";

            //logger.Info($"CurrenEnvironment:  {type}");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //如果设备已经绑定
            if (!RestService.CreateInstance().IsBinded())
            {
                var loginForm = new MainForm();
                if (loginForm.ShowDialog() == DialogResult.Cancel) return;
            }

            RestService.CreateInstance().Initialization();
            //_rest = RestService.CreateInstance();
            //_rest.Initialization();
            //_rest.Start(_rest, "http://127.0.0.1:8005/api");


            //Application.Run(new MainForm());
            //绑定成功后运行
            Application.Run(new Form2());
        }


        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Scripts.LogServer.AddMessage(e.ToString());
            logger.Error("UnhandledException" + e.ExceptionObject.ToString());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //Scripts.LogServer.AddMessage(e.ToString());
            logger.Error("ThreadException" + e.Exception.ToString());

        }
    }
}
