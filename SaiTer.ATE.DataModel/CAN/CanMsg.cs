using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class CanMsg
    {
        public int ObjectNo { get; set; }
        public string Direction { get; set; }
        public string CreateTimestamp { get; set; }
        /// <summary>
        /// 时间增量
        /// </summary>
        public string TimeIncrement { get; set; }   //add for 时间增量
        /// <summary>
        /// 帧ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// DLC
        /// </summary>
        public int Dlc { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string MsgData { get; set; }
        /// <summary>
        /// 报文翻译
        /// </summary>
        public string MsgText { get; set; }
    }
}
