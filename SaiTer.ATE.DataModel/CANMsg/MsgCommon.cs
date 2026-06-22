using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public abstract class MsgCommon
    {
        public abstract CanMsgRich DecodeMsgData(string symbol, List<byte> content);
    }
}
