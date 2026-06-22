using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN1004 : ConsistSummaryBase
    {
        public Consist_DN1004(BusinessBase business) : base(business)
        {

        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCEM = SelectData("CEM", dic);
            var dicCRM = SelectData("CRM", dic);
            var dicSPN2560 = SelectData_SPN2560(dicCRM, "AA");
            var dicBRM = SelectData("BRM", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "CEM";
                if (JudgeHaveMsg(chargerId, msgName, dicCEM))
                    continue;

                msgName = "CRM";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2560))
                    continue;

                Dictionary<int, List<CanMsgRich>>  dicBRM_New= SelectData_MutiEndAfter(dicBRM, dicSPN2560[chargerId]);
                if (JudgeHaveMsg(chargerId, "BRM", dicBRM_New))
                {
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机未使用传输协议功能接收BRM报文", "自首次发送SPN2560=AA的CRM报文起5s内",  false);
                }
                else
                {
                    MeasureFirstToLastWithinSec(dicSPN2560[chargerId], dicBRM_New[chargerId],5000, out bool isQualified, out string _ResultText);
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输协议功能接收BRM报文", "自首次发送SPN2560=AA的CRM报文起" + _ResultText +"内",  isQualified);
                }

                MeasureFirstToLastWithinSec(dicSPN2560[chargerId], dicSPN2560[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送该报文", "自首次发送SPN2560=AA的CRM报文起" + _ResultText2 + "内", isQualified2);



                msgName = "CRM";
                MeasureCommon(chargerId, msgName, dicSPN2560);

                MeasureFirstToFirstWithoutSec(dicSPN2560[chargerId], dicCEM[chargerId], 5000, out bool isQualified3, out string _ResultText3);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自首次发送SPN2560=AA的CRM报文起超过" + _ResultText3,  isQualified3);

                msgName = "CEM";
                MeasureCommon(chargerId, msgName, dicCEM);

            }
        }
    }
}
