using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business.ProtocolSummary
{
    public class Consist_DP1003 : ConsistSummaryBase
    {
        public Consist_DP1003(BusinessBase business) : base(business)
        {
        }

        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            try
            {
                var dicBRM = SelectData("BRM", dic);
                var dicCRM = SelectData("CRM", dic);
                var dicSPN2560 = SelectData_SPN2560(dicCRM, "00");


                foreach (int chargerId in dic.Keys)
                {
                    string msgName = "BRM";
                    if (JudgeHaveMsg(chargerId, msgName, dicBRM))
                        continue;
                    Business.ProcessDataResult(new List<int>() { chargerId }, "","充电机使用传输协议功能接收完成BRM报文",  true);

                    msgName = "SPN2560=00的CRM";
                    if (JudgeHaveMsg(chargerId, msgName, dicSPN2560))
                        continue;

                    long span = MeasureAStopWhileRcvB(dicSPN2560[chargerId], dicBRM[chargerId], out bool isQualified);
                    if (isQualified)
                    {
                        Business.ProcessDataResult(new List<int>() { chargerId }, "SPN2560=00的CRM报文", "充电机停止接收发送", true);
                    }
                    else
                        Business.ProcessDataResult(new List<int>() { chargerId }, "SPN2560=00的CRM报文", "充电机未停止接收发送", false);

                    msgName = "CRM";
                    MeasureCommon(chargerId, msgName, dicCRM);
                }
            }
            catch(Exception ex) { Log.Log.LogException(ex); }
        }
    }
}
