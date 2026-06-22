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
    /// 企标研测直流：输入过压保护试验
    /// </summary>
    public class QB_RT_DC_InputOverVolt : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        double InputVoltage = 260;
        double NormalVoltage = 220;
        double WaitTime = 20;

        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public QB_RT_DC_InputOverVolt(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //过压值(V)=260.00|恢复值(V)=220.00|等待断电时间(s)=20
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            NormalVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
            if (NormalVoltage > 220)
            {
                NormalVoltage = 220;
            }
            if (strParams.Length > 2)
            {
                //等待断电时间(s)
                WaitTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            }
            VoltageRate = Convert.ToDouble(TrialItem.ItemParams.Split('|')[0].Split('=')[1]);
            if (InputVoltage / NormalVoltage >= VoltageRate)
            {
                InputVoltage = NormalVoltage * VoltageRate;
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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + NormalVoltage + "|" + InputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

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
                        d1.Add(item, AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt.ToString("F2"));

                        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC)
                            d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString("F2"));
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                            d2.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压正常", "输入电压(V)", "-", "-");
                    ProcessDataTmp(d2, "输入电压正常", "输出电压(V)", "-", "-");

                    SetACSource(testWorkParam.lstIDs, InputVoltage);

                    SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");

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
                        double inputVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压异常", "异常输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, "输入电压异常", "保护后桩输出电压(V)", "0", "30");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    ProcessDataConnect("输入电压异常", "是否有告警提示");
                }
            }
        }


        public override void ProcessData()
        {

        }
    }
}
