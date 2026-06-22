using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP1002 : ConsistSummaryBase
    {
        public Consist_DP1002(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCHM = SelectData("CHM", dic);
            var dicCRM = SelectData("CRM", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "CHM";
                if (JudgeHaveMsg(chargerId, msgName, dicCHM))
                    continue;
                MeasureCommon(chargerId, msgName, dicCHM);

                msgName = "CRM";
                if (JudgeHaveMsg(chargerId, msgName, dicCRM))
                    continue;
                MeasureCommon(chargerId, msgName, dicCRM);
            }
        }
    }
}
