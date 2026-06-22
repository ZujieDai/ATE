using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3001 : ConsistSummaryBase
    {
        public Consist_DP3001(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBCS = SelectData("BCS", dic);
            var dicCRO = SelectData("CRO", dic);
            var dicSPN2830 = SelectData_SPN2830(dicCRO, "AA");
            var dicCCS = SelectData("CCS", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "BCS";
                if (JudgeHaveMsg(chargerId, msgName, dicBCS))
                    continue;
                Business.ProcessDataResult(new List<int>() { chargerId }, "能", "充电机使用传输协议功能否接收完成BCS报文", true);

                msgName = "SPN2830=AA的BRO";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2830))
                    continue;
                long span = MeasureAStopWhileRcvB(dicSPN2830[chargerId], dicBCS[chargerId], out bool isQualified);
                if (isQualified)
                {
                    Business.ProcessDataResult(new List<int>() { chargerId }, "是", "充电机接收到SPN2830=AA的BRO报文后，停止发送CML报文", true);
                }
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "否", "充电机接收到SPN2830=AA的BRO报文后，停止发送CML报文", false);

                msgName = "CCS";
                if (JudgeHaveMsg(chargerId, msgName, dicCCS))
                    continue;
                MeasureCommon(chargerId, msgName, dicCCS);
            }
        }
    }
}
