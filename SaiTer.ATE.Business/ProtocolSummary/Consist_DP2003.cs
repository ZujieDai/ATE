using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP2003 : ConsistSummaryBase
    {
        public Consist_DP2003(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBRO = SelectData("BRO", dic);
            var dicSPN2829 = SelectData_SPN2829(dicBRO, "AA");
            var dicCML = SelectData("CML", dic);
            var dicCRO = SelectData("CRO", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "SPN2829=AA的BRO";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2829))
                    continue;
                msgName = "CML";
                if (JudgeHaveMsg(chargerId, msgName, dicCML))
                    continue;
                
                long span = MeasureAStopWhileRcvB(dicCML[chargerId], dicSPN2829[chargerId], out bool isQualified);
                if (isQualified)
                {
                    Business.ProcessDataResult(new List<int>() { chargerId }, "停止发送CML报文", "充电机接收到SPN2829=AA的BRO报文后", true);
                }
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "未停止发送CML报文", "充电机接收到SPN2829=AA的BRO报文后，", false);

                msgName = "CRO";
                if (JudgeHaveMsg(chargerId, msgName, dicCRO))
                    continue;
                MeasureCommon(chargerId, msgName, dicCRO);
            }
        }
    }
}
