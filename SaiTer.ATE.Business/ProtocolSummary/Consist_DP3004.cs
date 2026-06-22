using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3004 : ConsistSummaryBase
    {
        public Consist_DP3004(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBSM = SelectSPN_DP3004Data("BSM", dic);
            var dicCSS01 = new Dictionary<int, List<CanMsgRich>>();
            foreach (int cid in dic.Keys)
                dicCSS01.Add(cid, new List<CanMsgRich>());
            var dicCSS = SelectData("CCS", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "SPN3092=10或SPN3093=10或SPN3094=10或SPN3095=10的BSM";
                if (JudgeHaveMsg(chargerId, msgName, dicBSM))
                    continue;

                dicCSS01[chargerId] = SelectData_SPN3929(dic[chargerId], "01", dicBSM[chargerId][0].ConsistMsg.ObjectNo);
                if (dicCSS01[chargerId].Count > 0)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机保持上一状态,对不可信状态数据包不做处理,按BMS需求输出", "SPN3929", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机未保持上一状态,对不可信状态数据包不做处理,按BMS需求输出", "SPN3929", true);

                msgName = "CCS";
                if (JudgeHaveMsg(chargerId, msgName, dicCSS))
                    continue;
                MeasureCommon(chargerId, msgName, dicCSS);
            }
        }
    }
}
