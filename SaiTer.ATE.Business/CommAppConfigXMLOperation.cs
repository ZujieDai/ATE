using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public static class CommAppConfigXMLOperation
    {
        static Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static bool SetValue(string sKey, string sValue)
        {
            try
            {
                if (!config.AppSettings.Settings.AllKeys.Contains(sKey))
                {
                    config.AppSettings.Settings.Add(sKey, sValue);
                }
                else
                {
                    config.AppSettings.Settings[sKey].Value = sValue;
                }
                //一定要记得保存，写不带参数的config.Save()也可以
                config.Save(ConfigurationSaveMode.Modified);
                //刷新，否则程序读取的还是之前的值（可能已装入内存）
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static string GetValue(string sKey, string sDefaultValue)
        {
            string sValue = sDefaultValue;
            try
            {
                if (config.AppSettings.Settings.AllKeys.Contains(sKey))
                {
                    sValue = config.AppSettings.Settings[sKey].Value;
                }
            }
            catch (Exception ex)
            {

            }
            return sValue;
        }
    }
}
