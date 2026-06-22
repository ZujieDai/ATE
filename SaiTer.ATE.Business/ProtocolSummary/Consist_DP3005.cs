using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3005 : ConsistSummaryBase
    {
        public Consist_DP3005(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBSM = SelectData("BSM", dic);
            var dicCSS = SelectData("CSS", dic);
            var dicBSM00_SPN3096 = SelectData_SPN3096(dicBSM, "00");
            var dicCSS00_SPN3929 = new Dictionary<int, List<CanMsgRich>>();
            foreach (int cid in dic.Keys)
                dicCSS00_SPN3929.Add(cid, new List<CanMsgRich>());
            var dicCSS01_SPN3929 = new Dictionary<int, List<CanMsgRich>>();
            foreach (int cid in dic.Keys)
                dicCSS01_SPN3929.Add(cid, new List<CanMsgRich>());
            var dicCST = SelectData("CST", dic);
            var dicBSM01_SPN3096 = SelectData_SPN3096(dicBSM, "01");
            var dicBSM00Special = new Dictionary<int, List<CanMsgRich>>();
            foreach (int cid in dic.Keys)
                dicBSM00Special.Add(cid, new List<CanMsgRich>());

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "SPN3096=00的BSM";
                if (JudgeHaveMsg(chargerId, msgName, dicBSM00_SPN3096))
                    continue;

                msgName = "SPN3929=00的CCS";
                dicCSS00_SPN3929[chargerId] = SelectData_SPN3929(dicCSS[chargerId], "00", dicBSM00_SPN3096[chargerId][0].ConsistMsg.ObjectNo);
                if (JudgeHaveMsg(chargerId, msgName, dicCSS00_SPN3929))
                    continue;

                bool isPuase = IsPauseOuputI(dicBSM00_SPN3096[chargerId], dicCSS00_SPN3929[chargerId]);
                if(isPuase)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "是", "充电机暂停输出电流", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "否", "充电机暂停输出电流", false);

                msgName = "SPN3929=01的CCS";
                dicCSS01_SPN3929[chargerId] = SelectData_SPN3929(dicCSS[chargerId], "01", dicCSS00_SPN3929[chargerId][0].ConsistMsg.ObjectNo);
                if (JudgeHaveMsg(chargerId, msgName, dicCSS01_SPN3929))
                    continue;

                long span = MeasureFirstToFirstWithinSec(dicCSS00_SPN3929[chargerId], dicCSS01_SPN3929[chargerId], 600000, out bool isQualified);
                if(isQualified)
                    Business.ProcessDataResult(new List<int>() { chargerId }, span.ToString(), "等待恢复充电时间(ms)", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, span.ToString(), "等待恢复充电时间(ms)", false);

                msgName = "CST";
                if (JudgeHaveMsg(chargerId, msgName, dicCST))
                    continue;

                msgName = "SPN3096=01的BSM";
                if (JudgeHaveMsg(chargerId, msgName, dicBSM01_SPN3096))
                    continue;

                msgName = "第二次SPN3096=00的BSM";
                //获取：时间大于最后一条SPN3906=01的BSM，且SPN3906=00的BSM
                dicBSM00Special[chargerId] = SelectData_SPN3096(dicBSM[chargerId], "00", dicBSM01_SPN3096[chargerId][0].ConsistMsg.ObjectNo);
                if (JudgeHaveMsg(chargerId, msgName, dicBSM00Special))
                    continue;

                span = MeasureFirstToFirstSpecial(dicBSM00Special[chargerId], dicCST[chargerId], 600000, out isQualified);
                if (isQualified)
                    Business.ProcessDataResult(new List<int>() { chargerId }, span.ToString(), "等待充电机发送CST报文时间(ms)", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, span.ToString(), "等待充电机发送CST报文时间(ms)", false);

                msgName = "CST";
                MeasureCommon(chargerId, msgName, dicCST);
            }
        }
    }
}
