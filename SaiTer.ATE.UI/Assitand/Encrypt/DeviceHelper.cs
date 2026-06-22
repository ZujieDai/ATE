using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.UI.Assitand.Encrypt
{
    public class DeviceHelper
    {
        /// <summary>
        /// 取网卡Mac地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            try
            {
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        /// <summary>
        /// 取CPU序列号
        /// </summary>
        /// <returns></returns>
        public static string GetCpuID()
        {
            try
            {
                string cpuInfo = "";//cpu序列号 
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        /// <summary>
        /// 取硬盘序列号
        /// </summary>
        /// <returns></returns>
        public static string GetDiskID()
        {
            try
            {
                String HDid = "";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                }
                moc = null;
                mc = null;
                return HDid;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        /// <summary>
        /// 取IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        //st=mo["IpAddress"].ToString(); 
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        /// <summary>
        /// 取登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            try
            {
                string str = "";
                str = Environment.UserName;
                return str;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        /// <summary>
        /// 取计算机名称
        /// </summary>
        /// <returns></returns>
        public static string GetComputerName()
        {
            try
            {
                return System.Environment.MachineName;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        /// <summary>
        /// 取物理内存
        /// </summary>
        /// <returns></returns>
        public static string GetTotalPhysicalMemory()
        {
            try
            {

                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["TotalPhysicalMemory"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "Unknow";
            }
        }

        /// <summary>
        /// 取MD5
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        //public static string GetMD5(string myString)
        //{
        //    MD5 md5 = new MD5CryptoServiceProvider();
        //    byte[] fromData = System.Text.Encoding.Unicode.GetBytes(myString);
        //    byte[] targetData = md5.ComputeHash(fromData);
        //    //-------再用md5加密
        //    //  byte[] targetData2 = md5.ComputeHash(targetData);
        //    string byte2String = null;
        //    for (int i = 0; i < targetData.Length; i++)
        //    {
        //        byte2String += targetData[i].ToString("x"); //把md5加密後轉做16進制
        //    }
        //    return byte2String;
        //}

        /// <summary>
        /// 取数字
        /// </summary>
        /// <param name="md5"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        //public static string GetNum(string md5, int len)
        //{
        //    Regex regex = new Regex(@"\d");
        //    MatchCollection listMatch = regex.Matches(md5);
        //    string str = "";
        //    for (int i = 0; i < len; i++)
        //    {
        //        str += listMatch[i].Value;
        //    }
        //    while (str.Length < len)
        //    {
        //        //不足补0
        //        str += "0";
        //    }
        //    return str;
        //}

        public static string GetMachineCode()
        {
            //CPU信息
            string cpuId = GetCpuID();
            //磁盘信息
            //string diskId = GetDiskID();
            //网卡信息
            //string MacAddress = GetMacAddress();

            //string m1 = GetMD5(cpuId + typeof(string).ToString());
            //string m2 = GetMD5(diskId + typeof(int).ToString());
            //string m3 = GetMD5(MacAddress + typeof(double).ToString());

            //string code1 = GetNum(cpuId, 8);
            //string code2 = GetNum(diskId, 8);
            //string code3 = GetNum(MacAddress, 8);

            //return code1 + code2 + code3;
            return cpuId;

        }
    }
}
