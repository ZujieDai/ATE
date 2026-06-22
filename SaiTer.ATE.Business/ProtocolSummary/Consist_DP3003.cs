using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3003 : ConsistSummaryBase
    {
        public Consist_DP3003(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBSM = SelectSPN_DP3003Data("BSM", dic);
            var dicCSS = SelectData("CCS", dic);
            var dicCST = SelectData("CST", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "SPN3090=01或SPN3090=10或SPN3091=01或SPN3091=10或SPN3092=01或SPN3093=01或SPN3094=01或SPN3095=01的BSM";
                if (JudgeHaveMsg(chargerId, msgName, dicBSM))
                    continue;
                msgName = "CCS";
                if (JudgeHaveMsg(chargerId, msgName, dicCSS))
                    continue;
                MeasureAStopWhileRcvB(dicCSS[chargerId], dicBSM[chargerId], out bool isQualified);
                if (isQualified)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机接收到异常BSM报文后", "停止发送CSS报文",  true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机接收到异常BSM报文后", "未停止发送CSS报文", false);

                msgName = "CST";
                if (JudgeHaveMsg(chargerId, msgName, dicCST))
                    continue;
                MeasureCommon(chargerId, msgName, dicCST);
            }
        }
    }
}
