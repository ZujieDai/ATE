using SaiTer.ATE.Business.ProtocolSummary;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class Consist_DP1001 : ConsistSummaryBase
    {
        public Consist_DP1001(BusinessBase business) : base(business)
        {
        }

        /// <summary>
        /// 判断提交结果
        /// </summary>
        public override void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic)
        {
            try
            {
                #region 张楷光代码 ----2024-1-19 戴祖杰修改
                //Dictionary<int, List<CanMsgRich>> dicCHM = SelectData("CHM", dic);

                //Dictionary<int, bool> dicIsFormatOK = new Dictionary<int, bool>();
                //Dictionary<int, long> dicMinPeriod = new Dictionary<int, long>();
                //Dictionary<int, long> dicMaxPeriod = new Dictionary<int, long>();

                //foreach (var item in dicCHM)
                //{
                //    bool IsFormatOK = true;
                //    long MinPeriod = 250, MaxPeriod = 250;
                //    foreach (int chargerId in dic.Keys)
                //    {
                //        if (JudgeHaveMsg(chargerId, "CHM", dicCHM))
                //            continue;

                //        for (int i = 1; i < item.Value.Count; i++)
                //        {
                //            CanMsgRich temp = item.Value[i];
                //            if (temp.Dlc != 3)
                //            {
                //                IsFormatOK = false;
                //            }
                //            DateTime thisTime = DateTime.ParseExact(temp.CreateTimestamp, "yyyyMMdd HH:mm:ss fff", null);
                //            DateTime previousTime = DateTime.ParseExact(item.Value[i - 1].CreateTimestamp, "yyyyMMdd HH:mm:ss fff", null);

                //            //if (temp.CreateTime) { }
                //            TimeSpan timeDifference = thisTime - previousTime;
                //            long milliseconds = (long)timeDifference.TotalMilliseconds;
                //            if (milliseconds >= 1000 || milliseconds <= 5)
                //                continue;
                //            if (milliseconds > dicMaxPeriod[item.Key])
                //            {
                //                MaxPeriod = milliseconds;
                //            }
                //            else if (milliseconds < dicMinPeriod[item.Key])
                //            {
                //                MinPeriod = milliseconds;
                //            }
                //        }
                //        dicIsFormatOK.Add(item.Key, IsFormatOK);
                //        dicMaxPeriod.Add(item.Key, MaxPeriod);
                //        dicMinPeriod.Add(item.Key, MinPeriod);
                //    }
                //}
                #endregion

                var dicCHM = SelectData("CHM", dic);

                foreach (int chargerId in dic.Keys)
                {
                    string msgName = "CHM";
                    if (JudgeHaveMsg(chargerId, msgName, dicCHM))
                        continue;
                    MeasureCommon(chargerId, msgName, dicCHM);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

    }
}
