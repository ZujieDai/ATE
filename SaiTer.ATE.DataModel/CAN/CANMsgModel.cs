using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    /// <summary>
    /// CAN报文模型
    /// </summary>
    public class CANMsgModel
    {
        /// <summary>
        /// 报文ID（HEX字符串）
        /// </summary>
        public string MsgID { get; set; }
        /// <summary>
        /// CAN报文内容（HEX字符串）
        /// </summary>
        public string MsgData { get; set; }

    }
}
