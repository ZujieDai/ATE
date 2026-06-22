using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP4001 : ConsistSummaryBase
    {
        public Consist_DP4001(BusinessBase business) : base(business)
        {
        }
        private string CSD = "CSD";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCSD = SelectData("CSD", dic);



            foreach (int chargerId in dic.Keys)
            {

                if (JudgeHaveMsg(chargerId, CSD, dicCSD))
                    continue;

                MeasureCommon(chargerId, CSD, dicCSD);

                Business.ProcessDataResult(new List<int>() { chargerId }, "充电机停止发送CSD，关闭辅助电源，充电结束", "", true);

            }
        }
    }
}
