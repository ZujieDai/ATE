using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.DataBaseModel
{
    /// <summary>
    /// 数据库设备配置表
    /// </summary>
    public class EquipmentConfigModel
    {
        /// <summary>
        /// 桩类型
        /// </summary>
        public int ChargerType { get; set; }
        /// <summary>
        /// 配置类型
        /// </summary>
        public string ConfigType { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string EquipmentName { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string Params1 { get; set; }

        /// <summary>
        /// 参数2
        /// </summary>
        public string Params2 { get; set; }

        /// <summary>
        /// 参数3
        /// </summary>
        public string Params3 { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}
