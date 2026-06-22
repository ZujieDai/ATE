using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP2001 : ConsistSummaryBase
    {
        public Consist_DP2001(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBCP = SelectMutiEndData("BCP", dic);
            var dicCRM = SelectData("CRM", dic);
            var dicSPN2560 = SelectData_SPN2560(dicCRM, "AA");
            var dicCML = SelectData("CML", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "BCP";
                if (JudgeHaveMsg(chargerId, msgName, dicBCP))
                    continue;
                Business.ProcessDataResult(new List<int>() { chargerId },"", "充电机使用传输协议功能接收完成BCP报文",  true);

                msgName = "SPN2560=AA的CRM";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2560))
                    continue;

                long span = MeasureAStopWhileRcvB(dicSPN2560[chargerId], dicBCP[chargerId], out bool isQualified);
                if (isQualified)
                {
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机停止接收发送", "SPN2560=AA的CRM报文", true);
                }
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机未停止接收发送", "SPN2560=AA的CRM报文",  false);

                msgName = "CML";
                if (JudgeHaveMsg(chargerId, msgName, dicCML))
                    continue;
                MeasureCommon(chargerId, msgName, dicCML);
            }
        }
    }
}
