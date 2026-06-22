using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public class CANPacket
    {
        /// <summary>
        /// 帧序号
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 枪号
        /// </summary>
        public int ChargeID { get; set; }
        /// <summary>
        /// 收发标志
        /// </summary>
        public string Mark { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public string RecvTime { get; set; }
        /// <summary>
        /// 时间增量
        /// </summary>
        public string TimeIncrement { get; set; }
        /// <summary>
        /// 帧ID
        /// </summary>
        public string FrameID { get; set; }
        /// <summary>
        /// DLC
        /// </summary>
        public string DLC { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// BMS报文翻译
        /// </summary>
        public string Explain { get; set; }
    }
}
