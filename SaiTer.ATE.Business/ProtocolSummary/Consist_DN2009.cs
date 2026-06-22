using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DN2009 : ConsistSummaryBase
    {
        private string BRO = "BRO";
        public Consist_DN2009(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCRO = SelectData("CRO", dic);
            var dicSPN2830 = SelectData_SPN2830(dicCRO, "00");

            var dicBRO = SelectData("BRO", dic);
            var dicSPN2829 = SelectData_SPN2829(dicBRO, "00");


            var Any = SelectMsg_CXX_Msg(dic);


            foreach (int chargerId in dic.Keys)
            {
                string msgName = "CRO";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2830))
                    continue;

                msgName = "BRO";
                if (JudgeHaveMsg(chargerId, msgName, dicSPN2829))
                    continue;


                if (JudgeHaveMsg(chargerId, "充电机", Any))
                    continue;

                MeasureFirstToLastLess(dicSPN2829[chargerId],Any[chargerId],1000, out bool isQualified, out string _ResultText);


                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机停止了通讯", "自首次接收到SPN2829=00的BRO报文起" + _ResultText + "内", isQualified);

            }
        }
    }
}
