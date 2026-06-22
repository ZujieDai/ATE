using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3006 : ConsistSummaryBase
    {
        public Consist_DP3006(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBST = SelectData("BST", dic);
            var dicCSS = SelectData("CSS", dic);
            var dicCST = SelectData("CST", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "BST";
                if (JudgeHaveMsg(chargerId, msgName, dicBST))
                    continue;

                msgName = "CSS";
                if (JudgeHaveMsg(chargerId, msgName, dicCSS))
                    continue;

                long span = MeasureAStopWhileRcvB(dicCSS[chargerId], dicBST[chargerId], out bool isQualified);
                if (isQualified)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "是", "充电机接收到BST报文后，停止发送CSS报文", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "否", "充电机接收到BST报文后，停止发送CSS报文", false);

                msgName = "CST";
                if (JudgeHaveMsg(chargerId, msgName, dicCST))
                    continue;
                MeasureCommon(chargerId, msgName, dicCST);
            }
        }
    }
}
