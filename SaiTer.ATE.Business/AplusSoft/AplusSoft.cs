using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.IDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage.PortType;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    class AplusSoft
    {
        //打开窗体需要引用
        [DllImport("User32.dll", EntryPoint = "SetParent")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);


        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern long SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);



        [DllImport("user32.dll", EntryPoint = "MoveWindow")]

        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        //  public static string APlusAddress = "D:\\Program Files\\A+\\Debug\\ST-9980A+.exe";
        private static string APlusAddress = "Debug\\ST-9980A+.exe";
        private static string APlusName = "ST-9980A+";//程序进程名称
        private static Process p = null;
        public static UdpIpPort APlusClient = new UdpIpPort();

        static public void OpenAPlusApp()
        {

            int ProgressCount = 0123456;//判断进程是否运行的标识
            Process[] prc = Process.GetProcesses();
            foreach (Process pr in prc) //遍历整个进程
            {
                if (APlusName == pr.ProcessName) //如果进程存在
                {

                    pr.Kill();
                    pr.Close();
                    break;
                }
            }
            if (ProgressCount != 0)//如果计数器不为0，说明所指定程序没有运行
            {
                try
                {
                    string BaseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                    //SystemEvent.SendCountDownTimer($"启动程序{BaseDirectory + APlusAddress}\n.", 0, 0);
                    p = new Process();
                    p.StartInfo.FileName = BaseDirectory + APlusAddress;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    p.Start();

                    while (p.MainWindowHandle.ToInt32() == 0)
                    {
                        System.Threading.Thread.Sleep(300);
                    }

                    //SetParent(p.MainWindowHandle, panel1.Handle);//SetParent(p.MainWindowHandle, panel1.Handle); //panel1.Handle为要显示外部程序的容器                     
                    ShowWindow(p.MainWindowHandle, (int)ProcessWindowStyle.Maximized);

                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                    SystemEvent.SendCountDownTimer(ex.Message + "", 0, 0);
                    //MessageBox.Show(d.Message + "", "提示!!!!");
                }
            }
            else
            {
                SystemEvent.SendCountDownTimer("对不起,本地已经有系统正在运行!\n.", 0, 0);
                //MessageBox.Show("对不起,本地已经有系统正在运行!\n.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static public void CloseAPlusApp()
        {

            Process[] prc = Process.GetProcesses();
            foreach (Process pr in prc) //遍历整个进程
            {
                if (APlusName == pr.ProcessName) //如果进程存在
                {
                    pr.Kill();
                    pr.Close();
                    break;
                }
            }

        }

        public static bool OpenAPlusAppInitialize(string ComName, int BaudRate, string OscilloscopeIP)
        {
            bool[] isBoolS = new bool[2];
            OpenAPlusApp();
            System.Threading.Thread.Sleep(2000); //()
            APlusClient.Connect();
            System.Threading.Thread.Sleep(1000); //()

            string ConfigStr = "SLAVECONNECT,ON," + ComName + "," + BaudRate;
            string BacksStr = "";

            for (int i = 0; i < 10; i++)
            {
                //BacksStr = APlusClient.SendRecieve(ConfigStr, "OK", "", 1000);
                BacksStr = APlusClient.SendRecieve(ConfigStr, "", "", 1000);
                if (BacksStr != "SLAVECONNECT,ON,OK")
                {
                    isBoolS[0] = false;
                    System.Threading.Thread.Sleep(3000); //()
                }
                else
                {
                    isBoolS[0] = true;
                    break;
                }
            }

            //示波板暂时不用
            if (!string.IsNullOrEmpty(OscilloscopeIP))
            {
                String[] StringS = OscilloscopeIP.Split(':');

                ConfigStr = "OSCCONNECT,ON," + StringS[0] + "," + StringS[1];//示波板卡
                for (int i = 0; i < 10; i++)
                {
                    BacksStr = APlusClient.SendRecieve(ConfigStr, "", "", 1000);
                    if (BacksStr != "OSCCONNECT,ON,OK")
                    {
                        isBoolS[1] = false;
                        System.Threading.Thread.Sleep(3000); //()
                    }
                    else
                    {
                        isBoolS[1] = true;
                        break;
                    }
                }
            }

            return isBoolS[0];

            if (isBoolS[0] && isBoolS[1])
            {
                return true;
            }
            else
            {
                return false;
            }


        }

    }
}
