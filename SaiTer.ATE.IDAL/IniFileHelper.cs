using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL
{
    /// <summary>
    /// 操作Ini配置文件
    /// </summary>
    public class IniFileHelper
    {
        /// <summary>
        /// 写文件操作
        /// </summary>
        /// <param name="section">节点值</param>
        /// <param name="key">键值</param>
        /// <param name="val">写入值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 读取文件操作
        /// </summary>
        /// <param name="section">节点值</param>
        /// <param name="key">键值</param>
        /// <param name="def">默认值</param>
        /// <param name="retVal">返回值</param>
        /// <param name="size">大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        //private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(string section, string key, string def, char[] retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);


        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="section">节点值</param>
        /// <param name="key">键值</param>
        /// <param name="def">默认值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string ReadIni(string section, string key, string defValue, string filePath)
        {

            int isize = 1024;
            StringBuilder retValue = new StringBuilder(isize);
            GetPrivateProfileString(section, key, defValue, retValue, isize, filePath);

            return retValue.ToString().Trim();
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="section">节点值</param>
        /// <param name="key">键值</param>
        /// <param name="val">写入值</param>
        /// <param name="filePath">文件路径</param>
        public static void WriteIni(string section, string key, string val, string filePath)
        {
            WritePrivateProfileString(section, key, val, filePath);
        }
    }
}
