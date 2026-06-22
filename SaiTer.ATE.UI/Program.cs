using SaiTer.ATE.UI.Assitand.Encrypt;
using SaiTer.ATE.UI.Assitand.PrjUI;
using System;
using System.Windows.Forms;
using SaiTer;
using Saiter;

namespace SaiTer.ATE.UI
{
    public class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Encryption en = new Encryption();//不能省略，否则出错


                UtilLicense m_objLicense = new UtilLicense(DeviceType.ST_ATEFrame);
                bool bRet = m_objLicense.CheckLicense(true); // 默认中文，默认启动校验
                if (bRet)
                {
                    Application.Run(new FrmLogin());
                }
                else
                {
                    return;
                }


                //if (bRet)
                //{

                //    System.Threading.Mutex mutex;
                //    bool ret;
                //    mutex = new System.Threading.Mutex(true, "SaiterAutoTestElectronic", out ret);

                //    if (!ret)
                //    {// 如果已经启动了程序，则不能再次启动
                //        MessageBox.Show("已有一个程序实例正在运行");
                //        Environment.Exit(0);
                //    }
                //    else
                //    {
                //        Application.Run(new FrmLogin());
                //    }
                //}
                //else
                //{
                //    Application.Run(new FrmRegister(strRetReg));
                //}
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            //int ret = TimeClass.InitReg();
            //string warning = Encryption.ReturnRegisterWarning(ret);
            //try
            //{
            //    if (ret == 0)
            //    {
            //        Application.Run(new FrmLogin());
            //    }
            //    else
            //    {
            //        Application.Run(new FrmRegister(warning));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Log.LogException(ex);
            //}
            //Application.Run(new FrmMain());    
        }
    }
}