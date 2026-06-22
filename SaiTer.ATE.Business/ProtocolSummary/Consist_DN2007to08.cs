using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN2007to08 : ConsistSummaryBase
    {
        private string CRO = "CRO";
        private string CEM = "CEM";
        public Consist_DN2007to08(BusinessBase business) : base(business)
        {
        }
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBRO = SelectData("BRO", dic);
            var dicSPN2829 = SelectData_SPN2829(dicBRO, "AA");

            var dicCEM = SelectData("CEM", dic);
            var dicCRO= SelectData("CRO", dic);



            foreach (int chargerId in dic.Keys)
            {
                string msgName = "BRO";
                if (JudgeHaveMsg(chargerId, "SPN2829 = AA的BRO", dicSPN2829))
                    continue;


                msgName = "CEM";
                if (JudgeHaveMsg(chargerId, msgName, dicCEM))
                    continue;

                msgName = "CRO";
                if (JudgeHaveMsg(chargerId, msgName, dicCRO))
                    continue;

                MeasureFirstToLastWithinSec(dicSPN2829[chargerId], dicCRO[chargerId], 5000, out bool isQualified, out string _ResultText);


                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送CRO报文", "自上一次接收到SPN2829=AA的BRO报文起" + _ResultText + "内",  isQualified);



                MeasureCommon(chargerId, CRO, dicCRO);

                MeasureFirstToFirstWithoutSec(dicSPN2829[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按发送CEM报文", "自上一次接收到SPN2829=AA的BRO报文起超过" + _ResultText2 ,   isQualified2);

                msgName = "CEM";
                MeasureCommon(chargerId, msgName, dicCEM);

            }
        }
    }
}
