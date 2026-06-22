using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.UI.Assitand.Encrypt
{
    class TimeClass
    {
        public static int InitReg()
        {
            /*检查注册表*/
            string SerialNumber = ReadSetting("", "SerialNumber", "-1");    // 读取注册表， 检查是否注册 -1为未注册        
            //return 0;   //add by Levi for register
            if (SerialNumber == "-1")
            {
                return 1;
            }


            /* 比较CPUid */
            string CpuId = GetSoftEndDateAllCpuId(1, SerialNumber);   //从注册表读取CPUid
            string CpuIdThis = Encrypt.DeviceHelper.GetMachineCode();           //获取本机CPUId         
            if (CpuId != CpuIdThis)
            {
                return 2;
            }

            /* 比较时间 */
            string NowDate = TimeClass.GetNowDate();
            string EndDate = TimeClass.GetSoftEndDateAllCpuId(0, SerialNumber);
            if (EndDate == "" || EndDate == null)   //add by Levi for register
            {
                return 4;
            }
            if (Convert.ToInt32(EndDate) - Convert.ToInt32(NowDate) < 0)
            {
                return 3;
            }

            return 0;


        }
        /*当前时间*/
        public static string GetNowDate()
        {
            string NowDate = DateTime.Now.ToString("yyyyMMdd");
            if (File.Exists("C:\\Windows\\System32\\config\\DRIVERS"))
            {
                FileInfo fileInfo = new FileInfo("C:\\Windows\\System32\\config\\DRIVERS");
                DateTime dt = fileInfo.LastAccessTime;
                string NowDate2 = dt.ToString("yyyyMMdd");
                NowDate = Convert.ToInt32(NowDate) > Convert.ToInt32(NowDate2) ? NowDate : NowDate2;
            }
            return NowDate;
        }

        /*写入注册表*/
        public static void WriteSetting(string Section, string Key, string Setting)  // name = key  value=setting  Section= path
        {
            string text1 = Section;
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\ATEFrame");//ATE框架

#if ST_9980AP_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test"); //ST-9980AP
#elif ST_9980A_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980A");//ST-9980A
#elif ST_9980BP
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980BP");//ST-9980B+
#elif ST_990_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_990A");//ST-990
#elif ST_990_DCAL
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_990AL");//ST-990
#elif ST_9980UA_AC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980UA_AC");//ST-9980UA_AC
#elif ST_9980EA_AC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980EA_AC");//ST-9980EA_AC
#elif ST_9980CA_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980CA_DC");//ST-9980CA_DC
#elif ST_990BL
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_990BL");//ST-990BL
#endif
            if (key1 == null)
            {
                return;
            }
            try
            {
                key1.SetValue(Key, Setting);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                return;
            }
            finally
            {
                key1.Close();
            }

        }

        /*读取注册表*/
        public static string ReadSetting(string Section, string Key, string Default)
        {
            if (Default == null)
            {
                Default = "-1";
            }
            string text2 = Section;
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\ATEFrame");

#if ST_9980AP_DC
            RegistryKey key1 = Registry.CurrentUser.OpenSubKey("Software\\SaiterTest\\Test");
#elif ST_9980A_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980A"); 
#elif ST_9980BP
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980BP");
#elif ST_990_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_990A");
#elif ST_990_DCAL
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_990AL");
#elif ST_9980UA_AC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980UA_AC");
#elif ST_9980EA_AC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980EA_AC");
#elif ST_9980CA_DC
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_9980CA_DC");
#elif ST_990BL
            RegistryKey key1 = Registry.CurrentUser.CreateSubKey("Software\\SaiterTest\\Test_990BL");
#endif
            if (key1 != null)
            {
                object obj1 = key1.GetValue(Key, Default);
                key1.Close();
                if (obj1 != null)
                {
                    if (!(obj1 is string))
                    {
                        return "-1";
                    }
                    string obj2 = obj1.ToString();
                    return obj2;
                }
                return "-1";
            }


            return Default;
        }

        /* 
        * i=1 得到 CPU 的id 
        * i=0 得到上次或者 开始时间 
        */
        public static string GetSoftEndDateAllCpuId(int i, string SerialNumber)
        {
            if (i == 1)
            {
                string cpuid = SerialNumber.Substring(0, 64);
                string decryptCpuId = Encryption.Decrypt(cpuid, Encryption.CRYPTO_KEY);
                return decryptCpuId;
            }
            if (i == 0)
            {
                string dateTime = SerialNumber.Substring(64);
                string decryptTime = Encryption.Decrypt(dateTime, Encryption.CRYPTO_KEY);
                return decryptTime;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
