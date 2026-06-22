using SaiTer.ATE.Controls;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaiTer.ATE.Manage
{

    public class ControlsManage
    {
        //public Dictionary<int, ControlsListManager> DitControlAssmeblyManager = null;
        public ControlsListManager ControlAssmeblyManager = null;

        public ControlsManage()
        {
            LoadAssmebly();
        }
        /// <summary>
        /// 加载反射
        /// </summary>
        private void LoadAssmebly()
        {
            try
            {

                //DitControlAssmeblyManager = new Dictionary<int, ControlsListManager>();

                XDocument _XDoc = XDocument.Load("xml\\EquipMentManage.xml");
                //实例化设备管理类
                EquipMentsManage _EquipMent = new EquipMentsManage();

                XElement item = _XDoc.Descendants("Controls").Elements("Control").First();
                ControlAssmeblyManager = new ControlsListManager();

                string[] StrClassName = item.Attribute("ControlClass").Value.ToString().Split('|');

                for (int i = 0; i < StrClassName.Length; i++)
                {
                    //将设备操作赋值给控制类的设备属性
                    ControlAssmeblyManager.SetControls(StrClassName[i], _EquipMent.EquipMentAssmeblyManager.Sessions);
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

        }
    }
}
