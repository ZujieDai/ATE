using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.DataBaseModel
{
    public class EquipMentCMD
    {
        /// <summary>
        /// 命令ID
        /// </summary>
        public int EquipMentCMDID { get; set; }
        /// <summary>
        /// 命令名称
        /// </summary>
        public string EquipMentCMDName { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public int EquipMentType { get; set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public string EquipMentModel { get; set; }
        /// <summary>
        /// 命令类型
        /// </summary>
        public int CMDType { get; set; }
        /// <summary>
        /// 针对的CAN的ID（HEX字符串）（非CAN类型的报文可以没有）
        /// </summary>
        public string CMDID_Can { get; set; }
        /// <summary>
        /// 命令内容（目前是HEX字符串）
        /// </summary>
        public string CMDContent { get; set; }
        /// <summary>
        /// 命令描述
        /// </summary>
        public string CMD_Description { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatTime { get; set; }

        public EquipMentCMD()
        {
            EquipMentCMDID = 1;
            EquipMentCMDName = "";
            EquipMentType = 1;
            EquipMentModel = "";
            CMDType = 1;
            CMDID_Can = "0018";
            CMDContent = "00000000000001";
            CMD_Description = "";
            CreatTime = "";
        }
    }
}
