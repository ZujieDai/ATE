using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN2001to02 : ConsistSummaryBase
    {
        public Consist_DN2001to02(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCEM = SelectData("CEM", dic);
            var dicCRM = SelectData("CRM", dic);
            var dicSPN2560 = SelectData_SPN2560(dicCRM, "AA");

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "CEM";
                if (JudgeHaveMsg(chargerId, msgName, dicCEM))
                    continue;

                msgName = "CRM";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2560))
                    continue;

                MeasureFirstToLastWithinSec(dicSPN2560[chargerId], dicSPN2560[chargerId], 5000, out bool isQualified, out string _ResultText);


                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送该报文", "自首次发送状态为AA的CRM报文起" + _ResultText + "内",  isQualified);



                msgName = "CRM";
                MeasureCommon(chargerId, msgName, dicSPN2560);

                MeasureFirstToFirstWithoutSec(dicSPN2560[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自首次发送状态为AA的CRM报文起" + _ResultText2 , isQualified2);

                msgName = "CEM";
                MeasureCommon(chargerId, msgName, dicCEM);

            }
        }
    }
}
