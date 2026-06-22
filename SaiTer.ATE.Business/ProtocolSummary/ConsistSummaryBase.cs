using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public abstract class ConsistSummaryBase
    {
        public const string Space = " ";
        /// <summary>
        /// 旧的话可能从配置文件里面读取AppSetting
        /// </summary>
        public OffsetConfig OffsetConfig = new OffsetConfig();
        public BusinessBase Business;
        public int TrialType { get => Business.TrialType; }
        public ConsistSummaryBase(BusinessBase business) 
        {
            Business = business;
        }

        /// <summary>
        /// 判断提交结果
        /// </summary>
        public abstract void CacleConsistRslt(Dictionary<int, List<CanMsgRich>> dic);
        /// <summary>
        /// 筛选单包解析后的CAN报文
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Equals(msgName))
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        public Dictionary<int, List<CanMsgRich>> SelectMutiEndData(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Equals(msgName) && temp.ConsistMsg.IsPackageEnd == 1)
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        public Dictionary<int, List<CanMsgRich>> SelectReadyOrRejectData(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Equals(msgName) && (temp.ConsistMsg.IsPackageReady == 1 || temp.ConsistMsg.IsPackageReady == 2))
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        public Dictionary<int, List<CanMsgRich>> SelectSPN_DP3003Data(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Equals(msgName))
                    {
                        if (temp.ConsistMsg.SPN3090 == "01" || temp.ConsistMsg.SPN3090 == "10" || temp.ConsistMsg.SPN3091 == "01" ||
                            temp.ConsistMsg.SPN3091 == "10" || temp.ConsistMsg.SPN3092 == "01" || temp.ConsistMsg.SPN3093 == "01" ||
                            temp.ConsistMsg.SPN3094 == "01" || temp.ConsistMsg.SPN3095 == "01")
                        {
                            if (!returnDic.ContainsKey(item.Key))
                            {
                                returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                            }
                            else
                            {
                                returnDic[item.Key].Add(temp);
                            }
                        }
                    }
                }
            }
            return returnDic;
        }
        public Dictionary<int, List<CanMsgRich>> SelectSPN_DP3004Data(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.SPN3092 == "10" || temp.ConsistMsg.SPN3093 == "10" ||
                        temp.ConsistMsg.SPN3094 == "10" || temp.ConsistMsg.SPN3095 == "10")
                    {
                        if (temp.ConsistMsg.MsgName.Equals(msgName))
                        {
                            if (!returnDic.ContainsKey(item.Key))
                            {
                                returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                            }
                            else
                            {
                                returnDic[item.Key].Add(temp);
                            }
                        }
                    }
                }
            }
            return returnDic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_SPN2560( Dictionary<int, List<CanMsgRich>> dic,string value)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.SPN2560.Contains(value))
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }

        /// <summary>
        /// 筛查SPN2829报文
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_SPN2829(Dictionary<int, List<CanMsgRich>> dic, string value)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.SPN2829.Contains(value))
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectMsg_CXX_Msg( Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Substring(0, 1) == "C")
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }

        /// <summary>
        /// 筛查SPN2830报文
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_SPN2830(Dictionary<int, List<CanMsgRich>> dic, string value)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.SPN2830.Contains(value))
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        /// <summary>
        /// 比较ObjectNo
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_BeforeMsg(Dictionary<int, List<CanMsgRich>> dic, List<CanMsgRich> msg)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.ObjectNo<msg[0].ObjectNo)
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }

        /// <summary>
        /// 筛查SPN2830报文
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_SPN3096(Dictionary<int, List<CanMsgRich>> dic, string value)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.SPN3096.Contains(value))
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }

        /// <summary>
        /// 筛查SPN3929报文
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<CanMsgRich> SelectData_SPN3929(List<CanMsgRich> lst, string value, int objNo)
        {
            var returnLst = new List<CanMsgRich>();
            foreach (var temp in lst)
            {
                if (temp.ConsistMsg.SPN3929.Contains(value) && temp.ConsistMsg.ObjectNo > objNo)
                {
                    returnLst.Add(temp);
                }
            }
            return returnLst;
        }

        /// <summary>
        /// 筛查SPN3096报文
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<CanMsgRich> SelectData_SPN3096(List<CanMsgRich> lst, string value, int objNo)
        {
            var returnLst = new List<CanMsgRich>();
            foreach (var temp in lst)
            {
                if (temp.ConsistMsg.SPN3096.Contains(value) && temp.ConsistMsg.ObjectNo > objNo)
                {
                    returnLst.Add(temp);
                }
            }
            return returnLst;
        }

        /// <summary>
        /// 判断字典集合中是否为空
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_MutiEndAfter(Dictionary<int, List<CanMsgRich>> dic, List<CanMsgRich> msg)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.ObjectNo > msg[0].ObjectNo&& temp.ConsistMsg.IsPackageEnd==1)
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectDataMsgMult(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Equals(msgName)&&temp.ConsistMsg.IsPackageReady!=2)
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        public Dictionary<int, List<CanMsgRich>> SelectDataMutiReady(string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.MsgName.Equals(msgName) && temp.ConsistMsg.IsPackageReady == 1)
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }
        /// <summary>
        /// IsPackageEnd==1
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public Dictionary<int, List<CanMsgRich>> SelectData_MutiEnd(Dictionary<int, List<CanMsgRich>> dic)
        {
            var returnDic = new Dictionary<int, List<CanMsgRich>>();
            foreach (var item in dic)
            {
                foreach (var temp in item.Value)
                {
                    if (temp.ConsistMsg.IsPackageEnd == 1)
                    {
                        if (!returnDic.ContainsKey(item.Key))
                        {
                            returnDic.Add(item.Key, new List<CanMsgRich>() { temp });
                        }
                        else
                        {
                            returnDic[item.Key].Add(temp);
                        }
                    }
                }
            }
            return returnDic;
        }

        public bool JudgeHaveMsg(int chargerId, string msgName, Dictionary<int, List<CanMsgRich>> dic)
        {
            if (!dic.ContainsKey(chargerId) || dic[chargerId].Count < 1)
            {
                Business.ProcessDataResult(new List<int>() { chargerId }, "-", $"未接收到{msgName}报文", false);
                return true;
            }
            return false;
        }

        public void MeasureCommon(int chargerId, string msgName, Dictionary<int, List<CanMsgRich>> dicCANData, bool isContent = false)
        {
            var newLstIDS = new List<int>() { chargerId };
            GetFormatJudge(newLstIDS, msgName, dicCANData[chargerId]);
            GetIntervalJudge(newLstIDS, msgName, dicCANData[chargerId]);
            GetLengthJudge(newLstIDS, msgName, dicCANData[chargerId]);
            if (isContent)
                GetContentJudge(newLstIDS, msgName, dicCANData[chargerId]);
        }

        /// <summary>
        /// 判断格式
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="_MsgName"></param>
        /// <param name="canMsgRiches"></param>
        public void GetFormatJudge(List<int> NewlstIDs, string _MsgName, List<DataModel.CAN.CanMsgRich> canMsgRiches)
        {
            string Format = _MsgName + "格式:";
            if (canMsgRiches != null && canMsgRiches.Count > 0)
            {
                DataModel.CAN.ConsistMsg first = canMsgRiches[0].ConsistMsg;
                DataModel.CAN.ConsistMsg last = canMsgRiches[canMsgRiches.Count - 1].ConsistMsg;
                string ret = string.Empty;
                string spn = string.Empty;
                if (_MsgName == "CRM")
                {
                    if (TrialType == (int)EmTrialType.DP1002 || TrialType == (int)EmTrialType.DN1001 || TrialType == (int)EmTrialType.DN1002 || TrialType == (int)EmTrialType.DN1003)
                    {
                        spn = first.SPN2560;
                        ret = "SPN2560=" + spn + Space;
                        if (SpnEqualStr(spn, "00"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2560", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2560", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN1004 || TrialType == (int)EmTrialType.DN2001 || TrialType == (int)EmTrialType.DN1001 || TrialType == (int)EmTrialType.DN2002)
                    {
                        spn = first.SPN2560;
                        ret = "SPN2560=" + spn + Space;
                        if (SpnEqualStr(spn, "AA"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2560", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2560", false);
                        }

                    }
                    else
                    {
                        Business.ProcessDataResult(NewlstIDs, ret, Format + "", true);
                    }
                }
                else if (_MsgName == "CEM")
                {
                    if (TrialType == (int)EmTrialType.DN1001 || TrialType == (int)EmTrialType.DN1002 || TrialType == (int)EmTrialType.DN1001 || TrialType == (int)EmTrialType.DN1003)
                    {
                        spn = first.SPN3921;
                        ret = "SPN3921=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3921", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3921", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN1004 || TrialType == (int)EmTrialType.DN1004 || TrialType == (int)EmTrialType.DN2002)
                    {
                        spn = first.SPN3922;
                        ret = "SPN3922=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3922", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3922", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN2003 || TrialType == (int)EmTrialType.DN2004 || TrialType == (int)EmTrialType.DN2005 || TrialType == (int)EmTrialType.DN2006 || TrialType == (int)EmTrialType.DN2007 || TrialType == (int)EmTrialType.DN2008)
                    {
                        spn = first.SPN3923;
                        ret = "SPN3923=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3923", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3923", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN3001 || TrialType == (int)EmTrialType.DN3003 || TrialType == (int)EmTrialType.DN3005 || TrialType == (int)EmTrialType.DN3007)
                    {
                        spn = first.SPN3924;
                        ret = "SPN3924=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3924", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3924", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN2010 || TrialType == (int)EmTrialType.DN3002 || TrialType == (int)EmTrialType.DN3004 || TrialType == (int)EmTrialType.DN3006 || TrialType == (int)EmTrialType.DN3008)
                    {
                        spn = first.SPN3925;
                        ret = "SPN3925=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3925", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3925", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN3009 || TrialType == (int)EmTrialType.DN3010)
                    {
                        spn = first.SPN3926;
                        ret = "SPN3926=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3926", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3926", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DN4001 || TrialType == (int)EmTrialType.DN4002 || TrialType == (int)EmTrialType.DN4003 || TrialType == (int)EmTrialType.DN4004)
                    {
                        spn = first.SPN3927;
                        ret = "SPN3927=" + spn + Space;
                        if (SpnEqualStr(spn, "01"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3927", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN3927", false);
                        }
                    }
                    else
                    {
                        Business.ProcessDataResult(NewlstIDs, ret, Format + "", true);
                    }
                }
                else if (_MsgName == "CRO")
                {
                    if (TrialType == (int)EmTrialType.DN2007)
                    {
                        spn = first.SPN2830;
                        ret = "SPN2830=" + spn + Space;
                        if (SpnEqualStr(spn, "00"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2830", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2830", false);
                        }
                    }
                    else if (TrialType == (int)EmTrialType.DP2003)
                    {
                        //string conclusion = SpnEqualParas();

                        //ret = "SPN2830=00或SPN2830=AA" +Space + conclusion;
                        Business.ProcessDataResult(NewlstIDs, ret, Format + "", true);
                    }
                    else if (TrialType == (int)EmTrialType.DN2010 || TrialType == (int)EmTrialType.DN3001 || TrialType == (int)EmTrialType.DN3002 || TrialType == (int)EmTrialType.DN3003 || TrialType == (int)EmTrialType.DN3004)
                    {
                        spn = first.SPN2830;
                        ret = "SPN2830=" + spn + Space;
                        if (SpnEqualStr(spn, "AA"))
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2830", true);
                        }
                        else
                        {
                            Business.ProcessDataResult(NewlstIDs, spn, Format + "SPN2830", false);
                        }
                    }
                    else
                    {
                        Business.ProcessDataResult(NewlstIDs, ret, Format + "", true);
                    }
                }
                else
                {
                    ret = "合格";
                    Business.ProcessDataResult(NewlstIDs, ret, Format.Replace(":", ""), true);
                }
            }
            else
            {
                //Business.ProcessDataResult(NewlstIDs, "", "", false);
            }

        }

        /// <summary>
        /// 判断周期
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="_MsgName"></param>
        /// <param name="canMsgRiches"></param>
        public void GetIntervalJudge(List<int> NewlstIDs, string _MsgName, List<DataModel.CAN.CanMsgRich> canMsgRiches)
        {
            try
            {
                long[] _Std = StandarNormalInterval(_MsgName);
                if (_MsgName == CanMsgId.BRM || _MsgName == CanMsgId.BCP || _MsgName == CanMsgId.BCS
                    || _MsgName == CanMsgId.BMV || _MsgName == CanMsgId.BSP)
                {
                    List<ConsistMsg> Consists = new List<ConsistMsg>();
                    List<long> _Intervals = GenerateIntervalList(_MsgName, canMsgRiches, ref Consists);
                    GetMinPeriodTestText(NewlstIDs, _MsgName, _Intervals, Consists, _Std);
                    GetMaxPeriodTestText(NewlstIDs, _MsgName, _Intervals, Consists, _Std);
                }
                else
                {
                    List<ConsistMsg> Consists = new List<ConsistMsg>();
                    foreach (var item in canMsgRiches)
                    {
                        Consists.Add(item.ConsistMsg);
                    }

                    List<long> _Intervals = Function.CalcInterval(Consists);

                    GetMinPeriodTestText(NewlstIDs, _MsgName, _Intervals, Consists, _Std);
                    GetMaxPeriodTestText(NewlstIDs, _MsgName, _Intervals, Consists, _Std);
                }
            }
            catch (Exception ex)
            {
                Business.SendException(ex);
            }
        }
        /// <summary>
        /// 判断长度
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="_MsgName"></param>
        /// <param name="canMsgRiches"></param>
        public void GetLengthJudge(List<int> NewlstIDs, string _MsgName, List<DataModel.CAN.CanMsgRich> canMsgRiches)
        {
            List<ConsistMsg> _Data = new List<ConsistMsg>();

            foreach (var item in canMsgRiches)
            {
                _Data.Add(item.ConsistMsg);
            }

            int std = 0;
            int dataLen = 0;
            string text;
            //获取标准长度
            switch (_MsgName)
            {
                case CanMsgId.BHM:
                    std = 2;
                    break;
                case CanMsgId.BRM:
                    std = 49;
                    break;
                case CanMsgId.BCL:
                    std = 5;
                    break;
                case CanMsgId.CSD:
                case CanMsgId.CML:
                case CanMsgId.CRM:
                    std = 8;
                    break;
                case CanMsgId.BSM:
                case CanMsgId.BSD:
                case CanMsgId.CTS:
                case CanMsgId.CCS://27930旧国标CCS为7，补充说明里修改为8
                    std = 7;
                    break;
                case CanMsgId.BST:
                case CanMsgId.CST:
                case CanMsgId.BEM:
                case CanMsgId.CEM:
                    std = 4;
                    break;
                case CanMsgId.CHM:
                    std = 3;
                    break;
                case CanMsgId.BCP:
                case CanMsgId.BCS:
                    std = 14;
                    break;
                case CanMsgId.BRO:
                case CanMsgId.CRO:
                    std = 1;
                    break;
            }

            //获取实际数据长度，多包与非多包计算方法不一样，多包以正文包数*7来计算，非多包以dlc为准
            if (_MsgName == CanMsgId.BRM || _MsgName == CanMsgId.BSP
                || _MsgName == CanMsgId.BCS || _MsgName == CanMsgId.BCP)
            {
                dataLen = GetDataLen_Special(_Data, dataLen);
            }

            else
            {
                dataLen = GetDataLen_Common(_Data, std);
                if (_MsgName == CanMsgId.BMT || _MsgName == CanMsgId.BMV)//长度不定，不作比较
                {
                    //string ret = _MsgName + "长度" + ":" + dataLen + ":"
                    //     + " ";
                    Business.ProcessDataResult(NewlstIDs, dataLen.ToString(), _MsgName + "长度", true);


                    return;
                }
            }


            if (dataLen == std)
            {
                //string ret = _MsgName + "长度" + ":" + dataLen + " ";

                dataLen = SpecialMutiQualifiedLen(_MsgName, std);
                Business.ProcessDataResult(NewlstIDs, dataLen.ToString(), _MsgName + "长度", true);
            }
            else
            {


                //string ret = _MsgName + "长度" + ":" + dataLen + " ";
                if (_MsgName == CanMsgId.CCS)//CCS长度为7和8都合格
                {
                    if (dataLen == 8)
                    {
                        Business.ProcessDataResult(NewlstIDs, dataLen.ToString(), _MsgName + "长度", true);
                    }
                    else
                    {
                        Business.ProcessDataResult(NewlstIDs, dataLen.ToString(), _MsgName + "长度", false);
                    }

                }
                else
                {
                    Business.ProcessDataResult(NewlstIDs, dataLen.ToString(), _MsgName + "长度", false);
                }
            }

        }
        /// <summary>
        /// 判断内容，目前只支持CST
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="_MsgName"></param>
        /// <param name="canMsgRiches"></param>
        public void GetContentJudge(List<int> NewlstIDs, string _MsgName, List<DataModel.CAN.CanMsgRich> canMsgRiches)
        {
            List<ConsistMsg> _Data = new List<ConsistMsg>();

            foreach (var item in canMsgRiches)
            {
                _Data.Add(item.ConsistMsg);
            }


            if (_Data != null && _Data.Count > 0)
            {
                CheckContentJudge(NewlstIDs, _MsgName, _Data);
            }
            else
            {
                SetFormatUnqualified();
            }

        }

        #region 协议一致性所需函数


        private void CheckContentJudge(List<int> NewlstIDs, string _MsgName, List<ConsistMsg> _Data)
        {
            ConsistMsg first = _Data[0];
            ConsistMsg last = _Data[_Data.Count - 1];
            string ret = string.Empty;
            string sData = string.Empty;
            string stmp = string.Empty;
            Business.ProcessDataResult(NewlstIDs, "数据：" + sData, "", true);
            if (_MsgName == "CST")
            {
                if (TrialType == (int)EmTrialType.DP3006 || TrialType == (int)EmTrialType.DP4001 || TrialType == (int)EmTrialType.DN4001 || TrialType == (int)EmTrialType.DN4002)//车辆主动中止
                {
                    sData = first.MsgData;
                    CST_Spn3521_EqualStr(NewlstIDs, first.MsgData, "00,00,,01");
                }
                else if (TrialType == (int)EmTrialType.DP3003)//充电桩主动中止（故障）
                {
                    sData = first.MsgData;
                    CST_Spn3521_EqualStr(NewlstIDs, first.MsgData, "00,00,01,00");

                }
                else if (TrialType == (int)EmTrialType.DP3005)//不判断内容
                {
                    sData = first.MsgData;
                    CST_Spn3521_EqualStr(NewlstIDs, first.MsgData, ",,,");
                }
                else if (TrialType == (int)EmTrialType.DP3007 || TrialType == (int)EmTrialType.DN3009 || TrialType == (int)EmTrialType.DN3010 || TrialType == (int)EmTrialType.DN4003 || TrialType == (int)EmTrialType.DN4004)//充电桩主动中止（人工）
                {
                    sData = first.MsgData;
                    CST_Spn3521_EqualStr(NewlstIDs, first.MsgData, "00,01,00,00");
                }
            }
        }
        private void CST_Spn3521_EqualStr(List<int> NewlstIDs, string sMsg, string sStd)
        {
            string[] stds = sStd.Split(',');
            if (stds.Length < 4) return;
            StringBuilder sbtmp = new StringBuilder();
            GBCANMsg gbcst = GetMsgCSTinfo(sMsg);
            List<Msg> msgs = new List<Msg>();
            msgs.Add(gbcst.CST_SPN3521_1);
            msgs.Add(gbcst.CST_SPN3521_2);
            msgs.Add(gbcst.CST_SPN3521_3);
            msgs.Add(gbcst.CST_SPN3521_4);
            msgs.Add(gbcst.CST_SPN3522_1);
            msgs.Add(gbcst.CST_SPN3522_2);
            msgs.Add(gbcst.CST_SPN3522_3);
            msgs.Add(gbcst.CST_SPN3522_4);
            msgs.Add(gbcst.CST_SPN3522_5);
            msgs.Add(gbcst.CST_SPN3522_6);

            msgs.Add(gbcst.CST_SPN3523_1);
            msgs.Add(gbcst.CST_SPN3523_2);
            int iLen = stds.Length;
            if (iLen > msgs.Count)
            {
                iLen = msgs.Count;
            }
            for (int i = 0; i < iLen; i++)//目前只判断3521的内容
            {
                if (stds[i] != "" && stds[i].Trim() != "" && stds[i] != msgs[i].MsgData)
                {
                    string text = msgs[i].MsgName + "：" + msgs[i].MsgData + " " + SetFormatUnqualified();
                    Business.ProcessDataResult(NewlstIDs, text, "", false);


                }
            }


        }

        /// <summary>
        /// 解析cst报文二进制，并排序
        /// </summary>
        /// <param name="Msg"></param>
        /// <returns></returns>
        public GBCANMsg GetMsgCSTinfo(string Msg)
        {
            //msg格式：40 00 F0 F0 
            string sTmp = "";
            string sSecTmp = "";
            int iTmp = 0;
            string cstInfo = "";
            Msg = Msg.Replace(" ", "");//去掉空格
            string[] info = new string[16];
            string[] info2 = new string[16];
            if (Msg == "" || Msg.Length != 8) return null;
            for (int i = 0; i < 4; i++)
            {
                if (i == 1)
                {
                    sTmp = Msg.Substring(2 * 2, 2);
                }
                else if (i == 2)
                {
                    sTmp = Msg.Substring(2 * 1, 2);
                }
                else
                {
                    sTmp = Msg.Substring(2 * i, 2);
                }
                iTmp = Convert.ToInt32(sTmp, 16);//转换成10进制数字
                sSecTmp = Convert.ToString(iTmp, 2).PadLeft(8, '0');//转换成2进制字符串
                for (int j = 0; j < 4; j++)
                {
                    cstInfo += sSecTmp.Substring(8 - ((j + 1) * 2), 2);
                }
            }
            //cstinfo格式：00000010000000000000111100001111
            for (int j = 0; j < 16; j++)
            {
                info[j] = GetCstDetail(cstInfo.Substring(2 * j, 2), j);
                info2[j] = cstInfo.Substring(2 * j, 2);
            }

            GBCANMsg gbcst = new GBCANMsg();
            gbcst.CST_SPN3521_1.MsgName = "达到充电机设定条件中止";
            gbcst.CST_SPN3521_1.MsgData = info2[0];
            gbcst.CST_SPN3521_1.MsgText = info[0];
            gbcst.CST_SPN3521_2.MsgName = "人工中止";
            gbcst.CST_SPN3521_2.MsgData = info2[1];
            gbcst.CST_SPN3521_2.MsgText = info[1];
            gbcst.CST_SPN3521_3.MsgName = "故障中止";
            gbcst.CST_SPN3521_3.MsgData = info2[2];
            gbcst.CST_SPN3521_3.MsgText = info[2];
            gbcst.CST_SPN3521_4.MsgName = "BMS 中止（收到 CST 帧）";
            gbcst.CST_SPN3521_4.MsgData = info2[3];
            gbcst.CST_SPN3521_4.MsgText = info[3];

            gbcst.CST_SPN3522_1.MsgName = "充电机过温";
            gbcst.CST_SPN3522_1.MsgData = info2[4];
            gbcst.CST_SPN3522_1.MsgText = info[4];
            gbcst.CST_SPN3522_2.MsgName = "充电连接器故障";
            gbcst.CST_SPN3522_2.MsgData = info2[5];
            gbcst.CST_SPN3522_2.MsgText = info[5];
            gbcst.CST_SPN3522_3.MsgName = "充电机内部过温";
            gbcst.CST_SPN3522_3.MsgData = info2[6];
            gbcst.CST_SPN3522_3.MsgText = info[6];
            gbcst.CST_SPN3522_4.MsgName = "电量不能传送";
            gbcst.CST_SPN3522_4.MsgData = info2[7];
            gbcst.CST_SPN3522_4.MsgText = info[7];
            gbcst.CST_SPN3522_5.MsgName = "充电机急停";
            gbcst.CST_SPN3522_5.MsgData = info2[8];
            gbcst.CST_SPN3522_5.MsgText = info[8];
            gbcst.CST_SPN3522_6.MsgName = "故障";
            gbcst.CST_SPN3522_6.MsgData = info2[9];
            gbcst.CST_SPN3522_6.MsgText = info[9];

            gbcst.CST_SPN3523_1.MsgName = "电流不匹配";
            gbcst.CST_SPN3523_1.MsgData = info2[12];
            gbcst.CST_SPN3523_1.MsgText = info[12];

            gbcst.CST_SPN3523_1.MsgName = "电压异常";
            gbcst.CST_SPN3523_1.MsgData = info2[13];
            gbcst.CST_SPN3523_1.MsgText = info[13];


            return gbcst;
        }

        public string GetCstDetail(string strSec, int iNum)
        {
            string sStdd = "达到充电机设定条件中止" + "," +
               "人工中止" + "," +
                "故障中止" + "," +
                "BMS 中止（收到 CST 帧）" + "," +
                "充电机过温,充电连接器故障" + "," +
                "充电机内部过温" + "," +
                "电量不能传送" + "," +
                "充电机急停" + "," +
                "故障" + ",,," +
                "电流不匹配" + "," +
                "电压异常" +
                ",,";
            string sDetail = "";
            if (strSec == "00" || strSec == "10")
            {
                switch (strSec)
                {
                    case "00":
                        sDetail = "正常";
                        break;
                    case "10":
                        sDetail = "不可信";
                        break;
                    default:
                        sDetail = "正常";
                        break;
                }
            }
            else
            {
                string[] sTmp = sStdd.Split(',');
                if (sTmp[iNum] != "")
                {
                    sDetail = sTmp[iNum];
                }
                else
                {
                    sDetail = strSec;
                }
            }

            return sDetail;
        }





        private int SpecialMutiQualifiedLen(string msgName, int std)
        {
            if (msgName == CanMsgId.BCP)
                return 13;
            else if (msgName == CanMsgId.BCS)
                return 9;
            else
                return std;
        }
        public long[] StandarNormalInterval(string _MsgName)
        {
            double offset = Convert.ToDouble(OffsetConfig.Std50ms) / 100F;
            double offset10ms = OffsetConfig.Std10ms;
            long[] lens = new long[5];
            switch (_MsgName)
            {
                case CanMsgId.BHM:
                case CanMsgId.CHM:
                case CanMsgId.CRM:
                case CanMsgId.BRM://
                case CanMsgId.CML:
                case CanMsgId.BRO:
                case CanMsgId.CRO:
                case CanMsgId.BCS://
                case CanMsgId.BSM://
                case CanMsgId.BSD:
                case CanMsgId.CSD:
                case CanMsgId.BEM:
                case CanMsgId.CEM:
                    lens[0] = (long)(250 * (1 - offset));
                    lens[1] = (long)(250 * (1 + offset));

                    return lens;
                //case CanMsgId.BCP:
                //    lens[0] = (long)(500 * (1 - offset));
                //    lens[1] = (long)(500 * (1 + offset));
                //    lens[2] = (long)(10 - offset10ms);
                //    lens[3] = (long)(10 + offset10ms);
                //    lens[4] = 500;
                //    return lens;
                case CanMsgId.BCP:
                case CanMsgId.CTS:
                    lens[0] = (long)(500 * (1 - offset));
                    lens[1] = (long)(500 * (1 + offset));
                    return lens;
                case CanMsgId.BCL:
                case CanMsgId.CCS:
                    lens[0] = (long)(50 * (1 - offset));
                    lens[1] = (long)(50 * (1 + offset));
                    return lens;
                case CanMsgId.BST:
                case CanMsgId.CST:
                    lens[0] = (long)(10 - offset10ms);
                    lens[1] = (long)(10 + offset10ms);
                    return lens;
                case CanMsgId.JPDC100:
                case CanMsgId.JPDC101:
                case CanMsgId.JPDC102:
                case CanMsgId.JPDC108:
                case CanMsgId.JPDC109:
                    lens[0] = (long)(100 - 10);
                    lens[1] = (long)(100 + 10);
                    return lens;

                default:
                    return lens;
            }
        }

        //根据TextId得到包数*7的字节长度
        private int GetDataLen_Special(List<ConsistMsg> lists, int std)
        {
            Hashtable ht = new Hashtable();
            if (lists == null || lists.Count == 0)
                return 0;

            int len = 0;
            for (int i = 0; i < lists.Count; i++)
            {
                if (lists[i].TextId != 0)
                {
                    int id = lists[i].TextId;
                    if (!ht.ContainsKey(id))
                    {
                        ht.Add(id, 1);
                        len = 1;
                    }
                    else
                    {
                        len++;
                        ht[id] = len;
                    }
                    //if (lists[i].MutiLength != std)
                    //    return lists[i].MutiLength;
                }
            }
            if (ht != null && ht.Count != 0)
            {
                foreach (int value in ht.Values)
                {
                    if (std != value * 7)
                        return value * 7;
                }
            }
            return std;
        }
        private int GetDataLen_Common(List<ConsistMsg> lists, int std)
        {

            var distinct = lists.GroupBy(r => r.Dlc);
            foreach (var item in distinct)
            {
                int itemKey = Convert.ToInt32(item.Key.ToString());
                if (std != itemKey)
                    return itemKey;
                else
                    return itemKey;
            }
            return 0;
        }


        public List<long> GenerateIntervalList(string _MsgName, List<DataModel.CAN.CanMsgRich> _Data, ref List<ConsistMsg> Consists)
        {
            try
            {
                if (_MsgName == CanMsgId.BRM || _MsgName == CanMsgId.BCP || _MsgName == CanMsgId.BCS
                    || _MsgName == CanMsgId.BMV || _MsgName == CanMsgId.BSP)
                {
                    List<long> _Intervals = CalPeriodPackageInterval(_Data, ref Consists);
                    return _Intervals;
                }
                else
                {
                    List<ConsistMsg> ConsistsNew = new List<ConsistMsg>();


                    foreach (var item in _Data)
                    {
                        ConsistsNew.Add(item.ConsistMsg);
                    }
                    Consists = ConsistsNew;

                    List<long> _Intervals = Function.CalcInterval(ConsistsNew);//一个完整多包里，包与包之间的间隔
                    return _Intervals;
                }
            }
            catch (Exception ex)
            {
                Business.SendException(ex);
                return null;
            }
        }
        private List<long> CalPeriodPackageInterval(List<DataModel.CAN.CanMsgRich> _Data, ref List<ConsistMsg> Consists)
        {
            try
            {
                var distinct = _Data.GroupBy(obj => obj.ConsistMsg.PackageId).Select(group => group.First()).ToList();
                int _DistinctCnt = distinct.Count;
                List<ConsistMsg> consists = new List<ConsistMsg>();


                foreach (var item in distinct)
                {
                    consists.Add(item.ConsistMsg);
                }

                List<long> period = Function.CalcInterval(consists);
                List<long> _Intervals = new List<long>();
                _Intervals.AddRange(period);
                Consists = consists;
                return _Intervals;
            }
            catch (Exception ex)
            {
                Business.SendException(ex);
                return null;
            }
        }
        public long MaxInterval(List<long> _Intervals)
        {
            if (_Intervals.Count == 0)
                return 0;
            return _Intervals.Max();
        }
        public long MinInterval(List<long> _Intervals)
        {
            if (_Intervals.Count == 0)
                return 0;
            return _Intervals.Min();
        }
        private void GetMinPeriodTestText(List<int> NewlstIDs, string _MsgName, List<long> _Intervals, List<ConsistMsg> Consists, long[] _Std)
        {
            //string text = string.Empty;

            try
            {
                string result;
                string min = MinInterval(_Intervals).ToString();
                //text = _MsgName + "最小周期" + ":" + min;
                if (IsMinPeriodOk(_Intervals, Consists, _Std))
                {

                    Business.ProcessDataResult(NewlstIDs, min.ToString(), _MsgName+"最小周期", true);
                }
                else
                {
                    Business.ProcessDataResult(NewlstIDs, min.ToString(), _MsgName+ "最小周期", false);

                }





            }
            catch (Exception ex)
            {
                Business.SendException(ex);
            }

        }
        private bool IsMinPeriodOk(List<long> _Intervals, List<ConsistMsg> Consists, long[] _Std)
        {
            //long[] std = StandarNormalInterval(consistId);
            if (OffsetConfig.IsStd == false)
            {
                if (Consists.Count == 1)
                {
                    if (MinInterval(_Intervals) == 0)
                        return true;
                }
            }
            if (MinInterval(_Intervals) >= _Std[0] && MinInterval(_Intervals) <= _Std[1])
                return true;
            else
                return false;
        }

        private bool IsMaxPeriodOk(List<long> _Intervals, List<ConsistMsg> Consists, long[] _Std)
        {
            //long[] std = StandarNormalInterval(consistId);
            if (OffsetConfig.IsStd == false)
            {
                if (Consists.Count == 1)
                {
                    if (MaxInterval(_Intervals) == 0)
                        return true;
                }
            }
            if (MaxInterval(_Intervals) >= _Std[0] && MaxInterval(_Intervals) <= _Std[1])
                return true;
            else
                return false;
        }
        private string GetMaxPeriodTestText(List<int> NewlstIDs, string _MsgName, List<long> _Intervals, List<ConsistMsg> Consists, long[] _Std)
        {
            string text = string.Empty;

            try
            {
                //string result;
                string max = MaxInterval(_Intervals).ToString();
                //text += _MsgName + "最大周期" + ":" + max + " ";
                if (IsMaxPeriodOk(_Intervals, Consists, _Std))
                {
                    Business.ProcessDataResult(NewlstIDs, max.ToString(), _MsgName + "最大周期", true);

                }
                else
                {
                    Business.ProcessDataResult(NewlstIDs, max.ToString(), _MsgName + "最大周期", false);

                }





            }
            catch (Exception ex)
            {
                Business.SendException(ex);
            }
            return text;
        }
        private bool SetFormatQualified()
        {

            return true; /*"合格";*/
        }
        private bool SetFormatUnqualified()
        {

            return false;/* "不合格";*/
        }
        private bool SpnEqualStr(string spn, string qulifiedStr)
        {
            if (spn == qulifiedStr)
                return SetFormatQualified();
            else
                return SetFormatUnqualified();
        }
        public long MeasureAStopWhileRcvB(List<CanMsgRich> aList, List<CanMsgRich> bList, out bool isQualified)
        {
            string earlier = aList[aList.Count - 1].CreateTimestamp;
            string later = bList[0].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= 0)
                isQualified = true;
            else
                isQualified = false;
            return span;
        }
        public long MeasureFirstToLastWithinSec(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified,out string _ResultText)
        {
            string earlier = earlierList[0].CreateTimestamp;
            string later = laterList[laterList.Count - 1].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= 0 && span <= TimeoutOffset(ms))
                isQualified = true;
            else
                isQualified = false;
            _ResultText = span.ToString() + "ms";
            return span;
        }
        public long MeasureFirstToLastLess(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified, out string _ResultText)
        {
            string earlier = earlierList[0].CreateTimestamp;
            string later = laterList[laterList.Count - 1].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span <= TimeoutOffset(ms))
            {
                if (span < 0)
                    span = 0;
                isQualified = true;
            }
            else
                isQualified = false;
            _ResultText = span.ToString() + "ms";
            return span;
        }
        public long MeasureFirstToFirstWithinSec(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified)
        {
            string earlier = earlierList[0].CreateTimestamp;
            string later = laterList[0].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= 0 && span <= ms)
                isQualified = true;
            else
                isQualified = false;
            return span;
        }
        public long MeasureFirstToFirstWithoutSec(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified, out string _ResultText)
        {
            string earlier = earlierList[0].CreateTimestamp;
            string later = laterList[0].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= ms && span <= TimeoutOffset(ms))
                isQualified = true;
            else
                isQualified = false;
            _ResultText = span.ToString() + "ms";
            return span;
        }
        public long MeasureLastToLastWithoutSec(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified, out string _ResultText)
        {
            string earlier = earlierList[earlierList.Count - 1].CreateTimestamp;
            string later = laterList[laterList.Count - 1].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span <= TimeoutOffset(ms))
            {
                if (span < 0)
                    span = 0;
                isQualified = true;
            }
            else
                isQualified = false;
            _ResultText = span.ToString() + "ms";
            return span;
        }
        public long MeasureLastToFirstWithoutSec(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified, out string _ResultText)
        {
            string earlier = earlierList[earlierList.Count - 1].CreateTimestamp;
            string later = laterList[0].CreateTimestamp;
            //string later = laterList[laterList.Count - 1].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= ms && span <= TimeoutOffset(ms))
                isQualified = true;
            else
                isQualified = false;
            _ResultText = span.ToString() + "ms";
            return span;
        }
        public long MeasureLastToLastWithinSec(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified, out string _ResultText)
        {
            string earlier = earlierList[earlierList.Count - 1].CreateTimestamp;
            string later = laterList[laterList.Count - 1].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= 0 && span <= TimeoutOffset(ms))
                isQualified = true;
            else
                isQualified = false;
            _ResultText = span.ToString() + "ms";
            return span;
        }
        public long TimeoutOffset(long timeout)
        {
            long std = 0;
            //double offset1s = Prj.Prj.MainController.Config.StandardSet.Std1s;
            //double offset5s = Prj.Prj.MainController.Config.StandardSet.Std5s;
            //double offset10s = Prj.Prj.MainController.Config.StandardSet.Std10s;
            double offset1s = OffsetConfig.Std1s;
            double offset5s =OffsetConfig.Std5s;
            double offset10s = OffsetConfig.Std10s;
            if (timeout == 1000)
                std = (long)(timeout + offset1s);
            else if (timeout == 5000)
                std = (long)(timeout + offset5s);
            else if (timeout >= 10000)
                std = timeout + (long)offset10s;
            return std;
        }
        public long MeasureFirstToFirstSpecial(List<CanMsgRich> earlierList, List<CanMsgRich> laterList, long ms, out bool isQualified)
        {
            string earlier = earlierList[0].CreateTimestamp;
            string later = laterList[0].CreateTimestamp;
            long low = ms - OffsetConfig.Std10min * 1000;
            long high = ms + OffsetConfig.Std10min * 1000;

            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= low && span <= high)
                isQualified = true;
            else
                isQualified = false;
            return span;
        }
        public bool IsPauseOuputI(List<CanMsgRich> bsm00, List<CanMsgRich> ccs00)
        {
            string earlier = bsm00[0].CreateTimestamp;
            string later = ccs00[0].CreateTimestamp;
            long span = Function.CalcIntervalByTwoPara(later, earlier);
            if (span >= 0)
                return true;
            else
                return false;
        }

        #endregion

    }
}
