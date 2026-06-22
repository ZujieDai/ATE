using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN3006_08 : ConsistSummaryBase
    {
        public Consist_DN3006_08(BusinessBase business) : base(business)
        {
        }
        private string BCL = "BCL";
        private string CCS = "CCS";
        private string CEM = "CEM";
        private string BCS = "BCS";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBCS = SelectData(BCS, dic);
            Dictionary<int, List<CanMsgRich>> dicBCS_New = SelectDataMsgMult(BCS, dicBCS);
            var dicCEM = SelectData(CEM, dic);
            var dicBCL = SelectData(BCL, dic);
            var dicCCS = SelectData(CCS, dic);



            foreach (int chargerId in dic.Keys)
            {
                if (JudgeHaveMsg(chargerId, BCS, dicBCS_New))
                    continue;

                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输协议功能接收完成BCS报文", "", true);

                if (JudgeHaveMsg(chargerId, CEM, dicCEM))
                    continue;

                if (JudgeHaveMsg(chargerId, BCL, dicBCL))
                    continue;


                if (JudgeHaveMsg(chargerId, CCS, dicCCS))
                    continue;

                MeasureLastToLastWithinSec(dicBCL[chargerId], dicCCS[chargerId], 1000, out bool isQualified, out string _ResultText);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机按周期发送CCS报文", "自上一次接收到BCL报文起" + _ResultText + "内",   isQualified);
                MeasureCommon(chargerId, CCS, dicCCS);

                MeasureLastToFirstWithoutSec(dicBCL[chargerId], dicCEM[chargerId], 5000, out bool isQualified2, out string _ResultText2);
                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发送CEM报文", "自上一次接收到BCL报文起超过" + _ResultText2 ,   isQualified2);

                MeasureCommon(chargerId, CEM, dicCEM);
            }
        }
    }
}
