using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP2002 : ConsistSummaryBase
    {
        public Consist_DP2002(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCLM = SelectData("CML", dic);

            foreach (int chargerId in dic.Keys)
            {
                string msgName = "CML";
                if (JudgeHaveMsg(chargerId, msgName, dicCLM))
                    continue;
                MeasureCommon(chargerId, msgName, dicCLM);
            }
        }
    }
}
