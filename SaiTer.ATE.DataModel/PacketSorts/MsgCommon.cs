using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XPCar.Prj.Model;
using XPCar.Common;

namespace XPCar.Protocol.Decode.Msg
{
    public abstract class MsgCommon
    {
        public abstract CanMsgRich DecodeMsgData(string symbol, List<byte> content);


    }
}
