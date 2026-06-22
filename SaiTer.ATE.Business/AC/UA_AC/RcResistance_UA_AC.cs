using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 枪头电阻测试美标（目前TB在用）
    /// </summary>
    public class RcResistance_UA_AC : BusinessBase
    {
        double maxp = 1.1;

        double minp = 1.1;


        double MinOpenResistance, MaxOpenResistance, MinCloseResistance, MaxCloseResistance;


        public RcResistance_UA_AC(int type)
        {
            TrialType = type;
        }
        public override void InitializeParams()
        {
            Init();
            double CloseResistance = 100;
            double OpenResistance = 3380;

            //数据库参数格式
            //误差(%)=8|机械锁锁止电阻值(Ω)=3500|机械锁解锁电阻值(Ω)=220
            string[] strParams = TrialItem.ResultParams.Split('|');
            //maxp = (100 + double.Parse(strParams[0].Split('=')[1])) / 100;
            //minp = (100 - double.Parse(strParams[0].Split('=')[1])) / 100;
            ////MinCloseResistance = double.Parse(strParams[1].Split('=')[1]) * minp;
            ////MaxCloseResistance = double.Parse(strParams[1].Split('=')[1]) * maxp;
            ////MinOpenResistance = double.Parse(strParams[2].Split('=')[1]) * minp;
            ////MaxOpenResistance = double.Parse(strParams[2].Split('=')[1]) * maxp;
            //MinOpenResistance = OpenResistance * minp;
            //MaxOpenResistance = OpenResistance * maxp;
            //MinCloseResistance = CloseResistance * minp;
            //MaxCloseResistance = CloseResistance * maxp;

            //MinOpenResistance = 465;
            //MaxOpenResistance = 494;
            //MinCloseResistance = 145;
            //MaxCloseResistance = 154;

            MinCloseResistance = double.Parse(strParams[0].Split('=')[1]);
            MaxCloseResistance = double.Parse(strParams[1].Split('=')[1]);
            MinOpenResistance = double.Parse(strParams[2].Split('=')[1]);
            MaxOpenResistance = double.Parse(strParams[3].Split('=')[1]);

        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                StartItemFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                // SetCPReresh();

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }
        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
        {
            SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
            _StopWatch.Reset();
            _StopWatch.Start();
            while (true)
            {
                testWorkParam.lstIDs.Clear();
                for (int i = 0; i < LstTrialData.Count; i++)
                {
                    if (LstTrialData[i].IsCheck)
                    {
                        if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                        {
                            if (!testWorkParam.lstIDs.Contains(LstTrialData[i].ChargerId))
                            {
                                testWorkParam.lstIDs.Add(LstTrialData[i].ChargerId);
                            }
                        }
                    }
                }
                //是否全部有结论
                if (testWorkParam.lstIDs.Count <= 0) break;
                //是否超时
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
                {
                    for (int i = 0; i < LstTrialData.Count; i++)
                    {
                        if (LstTrialData[i].IsCheck)
                        {
                            if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                            {
                                LstTrialData[i].TrialResult = EmTrialResult.Fail;
                                LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                                int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                                LstTrialData[i].PKID = LstChargerInfo[k].PKID;

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                List<bool> lst = GetKStatus16_Charging();
                lst[0] = false;
                lst[9] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, lst);
                Thread.Sleep(1000);
                //设置测试条件
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    int key = testWorkParam.lstIDs[i];
                    if (AllEquipStateData.DicACSource_StateData.Count == 1)
                    {
                        key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                    }
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                    d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                }
                SetConditionValue("供电电压(V)", d1);
                SetConditionValue("供电频率(Hz)", d2);

                //因为要测试电阻，强行改变为国标用来检测电阻
                ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, EmChargerType.Charger_GB_AC);
                Thread.Sleep(5000);

                //CountDownTimeInfo("请判断桩是否有电子锁！", 10, 2);
                //ProcessData();
                //if (testWorkParam.lstIDs.Count == 0)
                //{
                //    return;
                //}

                CountDownTimeInfo("请按住机械锁！", 5, 0);
                Dictionary<int, string> dic = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                    dic.Add(item, CCResistance.ToString("F2"));
                }
                ProcessDataTmp(dic, "枪头电阻", "按下机械锁", MinOpenResistance.ToString(), MaxOpenResistance.ToString());//解锁状态



                CountDownTimeInfo("请松开机械锁！", 5, 0);
                dic.Clear();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                    dic.Add(item, CCResistance.ToString("F2"));
                }
                ProcessDataTmp(dic, "枪头电阻", "松开机械锁", MinCloseResistance.ToString(), MaxCloseResistance.ToString());//锁止状态

                ////恢复导引类型来读取数据
                ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, LstChargerInfo[0].ChargerType);
                Thread.Sleep(2000);

            }
        }
        public override void ProcessData()
        {

            foreach (var item in DicManualVerifyResult)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                if (item.Value)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否有机械锁|-|-|有";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否有机械锁|-|-|无";
                    testWorkParam.lstIDs.Remove(item.Key);
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     

                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);


            }

        }
    }
}
