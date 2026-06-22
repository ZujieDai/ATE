using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL
{
    /// <summary>
    /// txt文件操作类
    /// </summary>
    public class TxtFileHelper
    {
        /// <summary>
        /// 创建txt文件（覆盖式创建）
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="_Path">路径</param>
        /// <returns></returns>
        public static bool CreateTxt(string text, string _Path)
        {
            FileStream fs = new FileStream(_Path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            //开始写入
            sw.Write(text);

            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
            return true;
        }

        public static bool AppendTxt(string sPath, string sTxt, int iType)
        {
            try
            {
                if (iType == 0)
                {
                    File.WriteAllText(sPath, sTxt + Environment.NewLine);
                }
                else
                {
                    File.AppendAllText(sPath, sTxt + Environment.NewLine);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public StringBuilder GetMsg(string sPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(File.ReadAllText(sPath, Encoding.UTF8));

            return sb;
        }
    }
}
