using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输入缺相保护试验(源自南网企标，目前河南产测使用)
    /// </summary>
    public class GB_PT_DC_InputOpenPhase_XJ_QB : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        double NormalVoltage = 220;
        double WaitTime = 20;

        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public GB_PT_DC_InputOpenPhase_XJ_QB(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //恢复值(V)=220.00|等待断电时间(s)=20
            string[] strParams = TrialItem.ResultParams.Split('|');
            NormalVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            if (NormalVoltage > 220)
            {
                NormalVoltage = 220;
            }
            if (strParams.Length > 2)
            {
                //等待断电时间(s)
                WaitTime = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
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
                SetACSource(lstIDs, NormalVoltage);

                SetCPReresh();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

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
                                //界面展示的数据项格式
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {
                    #region 充电前模拟交流源缺相故障

                    
                    CountDownTimeInfo("请手动模拟交流源缺相故障", 600, 0);
                    Thread.Sleep(5000);

                    

                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    ProcessDataConnect("输入电压异常", "是否有告警提示");

                    CountDownTimeInfo("请手动恢复交流源正常状态", 600, 0);

                    #endregion

                    return;//后面的充电中故障暂不执行

                    #region 充电中模拟交流源缺相故障

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();

                    Thread.Sleep(3000); //等待输出电压稳定
                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        //d1.Add(item, AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt_C.ToString("F2"));//不能采集交流源的电压
                        d1.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel3RMSVolt.ToString("F2"));//用功率分析仪的数据

                        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC)
                            d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString("F2"));
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                            d2.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压正常", "输入电压(V)", "-", "-");
                    ProcessDataTmp(d2, "输入电压正常", "输出电压(V)", "-", "-");

                    //ControlEquipMent.ACSource?.ACSource_SetOpenPhase(lstIDs);
                    //SendNoticeToUIAndTxtFile("已发送交流源C相电压缺相，等待交流源输出稳定。");
                    CountDownTimeInfo("请手动模拟交流源缺相故障", 600, 0);
                    Thread.Sleep(5000);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    d1 = new Dictionary<int, string>();
                    int count = 0;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                            for (int j = 0; j < WaitTime; j++)
                            {
                                if (voltage > 20)
                                {
                                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;

                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    Thread.Sleep(1000);
                                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                                    if (voltage < 20)
                                        break;
                                }
                            }
                        }
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                        {
                            for (int j = 0; j < WaitTime; j++)
                            {
                                voltage = AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                                if (voltage < 20)
                                {
                                    count++;
                                    if (count >= 3)
                                    {
                                        break;
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                            //voltage = 0;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        double inputVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt_C;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压异常", "异常输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, "输入电压异常", "保护后桩输出电压(V)", "0", "30");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    ProcessDataConnect("输入电压异常", "是否有告警提示");

                    CountDownTimeInfo("请手动恢复交流源正常状态", 600, 0);

                    #endregion
                }
            }
        }


        public override void ProcessData()
        {

        }
    }
}
