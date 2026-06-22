using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN3005_07 : ConsistSummaryBase
    {
        public Consist_DN3005_07(BusinessBase business) : base(business)
        {
        }
        private string CCS = "CCS";
        private string BCS = "BCS";
        private string CEM = "CEM";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBCS = SelectData(BCS, dic);
            Dictionary<int, List<CanMsgRich>> dicBCS_New = SelectDataMsgMult(BCS, dicBCS);
            var dicCEM = SelectData(CEM, dic);
            var dicCCS = SelectData(CCS, dic);



            foreach (int chargerId in dic.Keys)
            {

                if (JudgeHaveMsg(chargerId, CEM, dicCEM))
                    continue;
                if (JudgeHaveMsg(chargerId, BCS, dicBCS_New))
                    continue;


                Dictionary<int, List<CanMsgRich>> dicCCS_New = SelectData_BeforeMsg(dicCCS, dicCEM[chargerId]);
                if (JudgeHaveMsg(chargerId,CCS, dicCCS_New))
                    continue;

                MeasureLastToLastWithoutSec(dicBCS_New[chargerId], dicCCS_New[chargerId], 5000, out bool isQualified, out string _ResultText);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送CCS报文", "自上一次接收到BCS报文起" + _ResultText + "内", isQualified);
                MeasureCommon(chargerId, CCS, dicCCS_New);

                MeasureLastToFirstWithoutSec(dicBCS_New[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自上一次接收到BCS报文起超过" + _ResultText2 , isQualified2);

                MeasureCommon(chargerId, CEM, dicCEM);
            }
        }
    }
}
