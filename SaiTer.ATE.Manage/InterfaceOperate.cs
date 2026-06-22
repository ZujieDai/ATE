using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Manage
{
    public class InterfaceOperate
    {
        /// <summary>
        /// 控制设备对象
        /// </summary>
        private XmlInfoAndAssembly localIniInfoAssembly = null;
       
        /// <summary>
        /// 界面操作类
        /// </summary>
        private static InterfaceOperate _InterfaceOperate = null;
        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static InterfaceOperate GetInstance(XmlInfoAndAssembly iniInfoAssembly)
        {
            if (_InterfaceOperate == null)
            {
                _InterfaceOperate = new InterfaceOperate(iniInfoAssembly);
            }
            return _InterfaceOperate;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public InterfaceOperate(XmlInfoAndAssembly iniInfoAssembly)
        {
            localIniInfoAssembly = iniInfoAssembly;          
        }



    }
}
