using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN3009to10 : ConsistSummaryBase
    {
        public Consist_DN3009to10(BusinessBase business) : base(business)
        {
        }
        private string CST = "CST";
        private string CEM = "CEM";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {

            var dicCEM = SelectData(CEM, dic);

            var dicCST = SelectData(CST, dic);


            foreach (int chargerId in dic.Keys)
            {


                if (JudgeHaveMsg(chargerId, CEM, dicCEM))
                    continue;

                if (JudgeHaveMsg(chargerId, CST, dicCST))
                    continue;




                MeasureFirstToLastWithinSec(dicCST[chargerId], dicCST[chargerId], 5000, out bool isQualified, out string _ResultText);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送该报文", "自首次发送CST报文起" + _ResultText + "内",  isQualified);
                MeasureCommon(chargerId, CST, dicCST);

                MeasureFirstToFirstWithoutSec(dicCST[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自首次发送CST报文起超过" + _ResultText2 ,   isQualified2);

                MeasureCommon(chargerId, CEM, dicCEM);
            }
        }
    }
}
