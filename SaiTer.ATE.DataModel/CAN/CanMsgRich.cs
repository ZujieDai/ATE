using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class CanMsgRich : CanMsg
    {
        //public int ObjectNo;
        //public string Direction;
        //public DateTime CreateTime;
        //public string Id;
        //public int Dlc;
        //public string MsgData;
        //public string MsgText;

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
