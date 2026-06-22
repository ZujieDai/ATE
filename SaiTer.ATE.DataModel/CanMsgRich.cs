using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public class CanMsgRich
    {
        public int ObjectNo { get; set; }
        public string Direction { get; set; }
        public string CreateTimestamp { get; set; }
        public string TimeIncrement { get; set; }   //add for 时间增量
        public string Id { get; set; }
        public int Dlc { get; set; }
        public string MsgData { get; set; }
        public string MsgText { get; set; }

        public string Symbol;

        public DateTime CreateTime;
        public int MaxIndex;
        public double SpanTime;
        public ConsistMsg ConsistMsg;
        public CanMsgRich()
        {
            ConsistMsg = new ConsistMsg();
        }
    }
}
