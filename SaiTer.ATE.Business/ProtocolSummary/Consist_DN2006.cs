using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN2006 : ConsistSummaryBase
    {
        public Consist_DN2006(BusinessBase business) : base(business)
        {
        }
        private string BCP = "BCP";
        private string CML = "CML";
        private string CEM = "CEM";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCEM = SelectData("CEM", dic);
            var dicCML = SelectData("CML", dic);
            var dicBCP= SelectData("BCP", dic);


            foreach (int chargerId in dic.Keys)
            {

                Dictionary<int, List<CanMsgRich>> dicBCP_New = SelectData_MutiEnd(dicBCP);

                if (JudgeHaveMsg(chargerId, BCP, dicBCP_New))
                    continue;


                Business.ProcessDataResult(new List<int>() { chargerId },"充电机使用传输协议功能接收完成BCP报文", "", true);


                if (JudgeHaveMsg(chargerId, CEM, dicCEM))
                    continue;

                if (JudgeHaveMsg(chargerId, CML, dicCML))
                    continue;

                MeasureFirstToLastWithinSec(dicCML[chargerId], dicCML[chargerId], 5000, out bool isQualified, out string _ResultText);


                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送该报文", "自首次发送CML报文起" + _ResultText + "内",  isQualified);

                MeasureCommon(chargerId, CML, dicCML);

                MeasureFirstToFirstWithoutSec(dicCML[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自首次发送CML报文起超过" + _ResultText2 ,  isQualified2);

                MeasureCommon(chargerId, CEM, dicCEM);

            }
        }
    }
}
