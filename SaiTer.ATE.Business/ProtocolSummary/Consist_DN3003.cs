using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN3003 : ConsistSummaryBase
    {
        public Consist_DN3003(BusinessBase business) : base(business)
        {
        }
        private string BCS = "BCS";
        private string CRO = "CRO";
        private string CEM = "CEM";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBCS = SelectData(BCS, dic);
            Dictionary<int, List<CanMsgRich>> dicBCS_New = SelectDataMutiReady(BCS, dicBCS);
            var dicCEM = SelectData(CEM, dic);
            var dicCRO = SelectData(CRO, dic);
            var dicSPN2830 = SelectData_SPN2830(dicCRO, "AA");


            foreach (int chargerId in dic.Keys)
            {

                if (JudgeHaveMsg(chargerId, BCS, dicBCS_New))
                    continue;
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输协议功能接收完成BCS报文", "", true);

                if (JudgeHaveMsg(chargerId, CEM, dicCEM))
                    continue;

                if (JudgeHaveMsg(chargerId, CRO, dicSPN2830))
                    continue;

                MeasureFirstToLastWithinSec(dicSPN2830[chargerId], dicSPN2830[chargerId], 5000, out bool isQualified, out string _ResultText);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送该报文", "自首次发送CRO报文起" + _ResultText + "内",   isQualified);
                MeasureCommon(chargerId, CRO, dicSPN2830);

                MeasureFirstToFirstWithoutSec(dicSPN2830[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自首次发送CRO报文起超过" + _ResultText2 , isQualified2);

                MeasureCommon(chargerId, CEM, dicCEM);
            }
        }
    }
}
