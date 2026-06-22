using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输入电压异常检测（包含输入过压、输入欠压）
    /// </summary>
    public class InputVoltageError : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        double InputVoltage = 220;  //异常电压
        double NormalVoltage = 220; //额定电压
        double RebcoverVoltage = 220; //恢复电压
        double WaitTime = 20;

        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public InputVoltageError(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //过压值(V)=268.00|恢复电压值(V)=254.00|额定电压值(V)=220.00
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            RebcoverVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
            //if (NormalVoltage > 220)
            //{
            //    NormalVoltage = 220;
            //}
            VoltageRate = Convert.ToDouble(TrialItem.ItemParams.Split('|')[0].Split('=')[1]);
            if (InputVoltage / NormalVoltage >= VoltageRate)
            {
                InputVoltage = NormalVoltage * VoltageRate;
            }
            if(strParams.Length > 2)
            {
                //等待断电时间(s)
                WaitTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            }
            if (strParams.Length > 3)
            {
                NormalVoltage = Convert.ToDouble(strParams[3].Split('=')[1]);
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
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                //    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //}

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

                    ////闭合开关S2，启动充电
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

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
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC || LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                            d2.Add(item, AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage.ToString("F2"));
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                            d2.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压正常", "输入电压(V)", "-", "-");
                    ProcessDataTmp(d2, "输入电压正常", "输出电压(V)", "-", "-");

                    //ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, InputVoltage);
                    SetACSource(testWorkParam.lstIDs, InputVoltage);
                    //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

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
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC || LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            for (int j = 0; j < WaitTime; j++)
                            {
                                voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                                if (voltage < 20)
                                {
                                    count++;
                                    if (count >= 3)
                                    {
                                        SendNoticeToUIAndTxtFile("充电桩触发保护。");
                                        break;
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                            //voltage = 0;
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
                        double inputVolt = AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压异常", "异常输入电压(V)", "-", "-");
                    if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                        LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                        LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        ProcessDataTmp(Data_Tmp, "输入电压异常", "保护后桩输出电压(V)", "0", "30");
                    else
                        ProcessDataTmp(Data_Tmp, "输入电压异常", "保护后桩输出电压(V)", "0", "60");

                    CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                    ProcessDataConnect("输入电压异常", "是否有告警提示");

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, (InputVoltage / NormalVoltage * 100).ToString("F2"));
                    }
                    if(InputVoltage > 220)
                    {
                        ProcessDataTmp(d1, "输入电压异常", "过压比例(%)", "115", "-");
                    }
                    else
                    {
                        ProcessDataTmp(d1, "输入电压异常", "欠压比例(%)", "-", "85");
                    }

                    SendNoticeToUIAndTxtFile("恢复正常电压：" + RebcoverVoltage + "V，等待交流源输出稳定。");
                    //ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, NormalVoltage);
                    SetACSource(testWorkParam.lstIDs, RebcoverVoltage);
                    //ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                    //这个是有恢复测试的
                    if (TrialType == (int)EmTrialType.输入过压保护及恢复测试
                        || TrialType == (int)EmTrialType.输入欠压保护及恢复测试
                        || TrialType == (int)EmTrialType.输入过压测试_欧标
                        || TrialType == (int)EmTrialType.输入欠压测试_欧标)
                    {
                        Thread.Sleep(5000);

                        d1 = new Dictionary<int, string>();
                        Data_Tmp = new Dictionary<int, string>();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            double voltage = 0;
                            d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt.ToString("F2"));
                            if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC)
                            {
                                for (int j = 0; j < WaitTime; j++)
                                {
                                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                                    if (voltage < 80)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                            {
                                for (int j = 0; j < WaitTime; j++)
                                {
                                    voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                                    if (voltage < 80)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                            {
                                SetCPRersh_EUDC();
                                CheckSwipingCard(testWorkParam.lstIDs);
                                for (int j = 0; j < WaitTime; j++)
                                {
                                    voltage = AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                                    if (voltage < 80)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        }
                        ProcessDataTmp(d1, "输入电压异常", "异常输入电压(V)", "-", "-");
                        ProcessDataTmp(Data_Tmp, "输入电压正常", "恢复后桩输出电压(V)", "60", "-");

                        SetACSource(testWorkParam.lstIDs, NormalVoltage);
                        ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    }


                    Thread.Sleep(2000);
                }
            }
        }


        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].ItemName = iIndex.ToString();

                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                    string strResult = "未停机";
                    if (volate < 50)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        strResult = "已停机";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //测试时间|BMS需求电压|输入电压|是否停机|测试结果     
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + NormalVoltage + "|" + InputVoltage + "|" + strResult;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
