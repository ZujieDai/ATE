using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3007 : ConsistSummaryBase
    {
        public Consist_DP3007(BusinessBase business) : base(business)
        {
        }
        private string CST = "CST";
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicCST = SelectData("CST", dic);



            foreach (int chargerId in dic.Keys)
            {

                if (JudgeHaveMsg(chargerId, CST, dicCST))
                    continue;

                MeasureCommon(chargerId, CST, dicCST);


            }
        }
    }
}
