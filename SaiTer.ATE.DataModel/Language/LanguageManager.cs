using ConfigurationSettings;
using SaiTer.ATE.DataModel.Properties;
using SaiTer.ATE.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public static class LanguageManager
    {
        public static string strDefaultLanguage = "";
        public static string strDefaultLanguageValue = "";
        public static Dictionary<string, string> DClanguage = new Dictionary<string, string>();
        public static List<string> Nativevalue = new List<string>();
        static LanguageManager()
        {
            try
            {

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                StartUpConfigSection section = (StartUpConfigSection)config.GetSection(StartUpConfigSection.XmlConfigKey);

                strDefaultLanguage = section.LanguageConfigure.Default;

                System.Resources.ResourceManager _rm = new System.Resources.ResourceManager("SaiTer.ATE.DataModel.Properties.Resources", typeof(Resources).Assembly);
                strDefaultLanguageValue = _rm.GetString(strDefaultLanguage.Replace("-", string.Empty));

                List<string> value = new List<string>();

                if (section.LanguageConfigure.Count > 0)
                {
                    // Add locale list.
                    foreach (LanguageConfigEntry language in section.LanguageConfigure)
                    {
                        value.Add(language.Name);

                        string name = new CultureInfo(language.Name).NativeName;
                        Nativevalue.Add(name);

                        DClanguage.Add(name, language.Name);
                    }
                }

                //初始化
                StringResources.Resource.Culture = new CultureInfo(strDefaultLanguage);

                _rm = StringResources.Resource.ResourceManager;

                Thread.CurrentThread.CurrentUICulture = StringResources.Resource.Culture;
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }


        private static ResourceManager _rm = StringResources.Resource.ResourceManager;
        public static string GetByKey(string key)
        {
            return _rm.GetString(key);
        }


        public static void ChangeLanguage(string language)
        {
            string key = DClanguage[language];

            StringResources.Resource.Culture = new CultureInfo(key);

            string name = new CultureInfo(key).NativeName;

            _rm = StringResources.Resource.ResourceManager;

            Thread.CurrentThread.CurrentUICulture = StringResources.Resource.Culture;
        }
    }
}
