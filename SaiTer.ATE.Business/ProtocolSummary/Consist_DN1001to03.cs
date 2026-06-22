using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN1001to03 : ConsistSummaryBase
    {
        public Consist_DN1001to03(BusinessBase business) : base(business)
        {

        }
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCEM = SelectData("CEM", dic);
            var dicCRM = SelectData("CRM", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "CEM";
                if (JudgeHaveMsg(chargerId, msgName, dicCEM))
                    continue;

                msgName = "CRM";

                Dictionary<int, List<CanMsgRich>> dicCRM_New = SelectData_BeforeMsg(dicCRM, dicCEM[chargerId]);
                if (JudgeHaveMsg(chargerId, "CRM", dicCRM_New))
                    continue;

                MeasureFirstToLastWithinSec(dicCRM_New[chargerId], dicCRM_New[chargerId], 5000, out bool isQualified, out string _ResultText);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送该报文", "自首次发送CRM报文起" + _ResultText + "内",  isQualified);               
                MeasureCommon(chargerId, msgName, dicCRM_New);

                MeasureFirstToFirstWithoutSec(dicCRM_New[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自首次发送CRM报文起超过" + _ResultText2 ,isQualified2);
                msgName = "CEM";
                MeasureCommon(chargerId, msgName, dicCEM);

            }
        }


    }
}
