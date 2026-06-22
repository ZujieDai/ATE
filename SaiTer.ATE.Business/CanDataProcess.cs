using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public partial class BusinessBase
    {


        ///// <summary>
        ///// 判断提交结果
        ///// </summary>
        //public void CacleConsistRslt(EmTrialType em)
        //{
        //    try
        //    {
        //        switch (em)
        //        {
        //            case EmTrialType.协议一致性试验_DP1001:
        //                Dictionary<int, List<DataModel.CAN.CanMsgRich>> dic = ControlEquipMent.BMS.GetCANDATA(testWorkParam.lstIDs);
        //                Dictionary<int, List<DataModel.CAN.CanMsgRich>> dicCHM = new Dictionary<int, List<DataModel.CAN.CanMsgRich>>();
        //                foreach (var item in dic)
        //                {
        //                    foreach (var temp in item.Value)
        //                    {
        //                        if (temp.Symbol.Equals("CHM"))
        //                        {
        //                            if (!dicCHM.ContainsKey(item.Key))
        //                            {
        //                                List<DataModel.CAN.CanMsgRich> lst = new List<DataModel.CAN.CanMsgRich>();
        //                                lst.Add(temp);
        //                                dicCHM.Add(item.Key, lst);
        //                            }
        //                            else
        //                            {
        //                                dicCHM[item.Key].Add(temp);
        //                            }
        //                        }
        //                    }
        //                }
        //                Dictionary<int, bool> dicIsFormatOK = new Dictionary<int, bool>();
        //                Dictionary<int, long> dicMinPeriod = new Dictionary<int, long>();
        //                Dictionary<int, long> dicMaxPeriod = new Dictionary<int, long>();

        //                foreach (var item in dicCHM)
        //                {
        //                    dicIsFormatOK.Add(item.Key, true);
        //                    dicMinPeriod.Add(item.Key, 250);
        //                    dicMaxPeriod.Add(item.Key, 250);

        //                    for (int i = 1; i < item.Value.Count; i++)
        //                    {
        //                        CanMsgRich temp = item.Value[i];
        //                        bool isFormatOK = true;
        //                        if (temp.Dlc != 3)
        //                        {
        //                            if (!dicIsFormatOK.ContainsKey(item.Key))
        //                            {
        //                                isFormatOK = false;
        //                                dicIsFormatOK.Add(item.Key, isFormatOK);
        //                            }
        //                            else
        //                            {
        //                                dicIsFormatOK[item.Key] = false;
        //                            }
        //                        }
        //                        DateTime thisTime = DateTime.ParseExact(temp.CreateTimestamp, "yyyyMMdd HH:mm:ss fff", null);
        //                        DateTime previousTime = DateTime.ParseExact(item.Value[i - 1].CreateTimestamp, "yyyyMMdd HH:mm:ss fff", null);

        //                        //if (temp.CreateTime) { }
        //                        TimeSpan timeDifference = thisTime - previousTime;
        //                        long milliseconds = (long)timeDifference.TotalMilliseconds;
        //                        if (milliseconds > dicMaxPeriod[item.Key])
        //                        {
        //                            dicMaxPeriod[item.Key] = milliseconds;
        //                        }
        //                        else if (milliseconds < dicMinPeriod[item.Key])
        //                        {
        //                            dicMinPeriod[item.Key] = milliseconds;
        //                        }
        //                    }
        //                }


        //                break;
        //        }
        //    }
        //    catch (Exception ex) { Log.Log.LogException(ex); }
        //}



    }
}
