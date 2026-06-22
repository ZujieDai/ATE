using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP3002 : ConsistSummaryBase
    {
        public Consist_DP3002(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            var dicBMV = SelectReadyOrRejectData("BMV", dic);
            var dicBMT = SelectReadyOrRejectData("BMT", dic);
            var dicBSP = SelectReadyOrRejectData("BSP", dic);

            foreach (int chargerId in dic.Keys)
            {
                if (dicBMV.ContainsKey(chargerId) && dicBMV[chargerId].Count > 0)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输功能接收BMV报文或放弃连接", "BMV报文", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输功能接收BMV报文", "BMV报文", true);

                if (dicBMT.ContainsKey(chargerId) && dicBMT[chargerId].Count > 0)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输功能接收BMT报文或放弃连接", "BMT报文", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输功能接收BMT报文", "BMT报文", true);

                if (dicBSP.ContainsKey(chargerId) && dicBSP[chargerId].Count > 0)
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输功能接收BSP报文或放弃连接", "BSP报文", true);
                else
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机使用传输功能接收BSP报文", "BSP报文", true);

            }
        }
    }
}
