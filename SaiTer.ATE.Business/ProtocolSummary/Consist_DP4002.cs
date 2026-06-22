using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP4002 : ConsistSummaryBase
    {
        public Consist_DP4002(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            foreach (int chargerId in dic.Keys)
            {
                int ChargeState = BusinessBase.ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[chargerId].ChargingState);
                if(ChargeState!=9)
                {
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机无法响应充电,需重新插拔充电连接装置后才能继续充电", "", true);
                }
                else
                {
                    Business.ProcessDataResult(new List<int>() { chargerId }, "充电机发生故障，未停止充电", "", true);
                }
            }
        }
    }
}
