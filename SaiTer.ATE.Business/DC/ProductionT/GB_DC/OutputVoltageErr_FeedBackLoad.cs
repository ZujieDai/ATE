using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using System.Runtime.InteropServices;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 使用回馈负载输出电压异常（输出过压、输出欠压）
    /// </summary>
    public class OutputVoltageErr_FeedBackLoad : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        double OutputVoltage = 700;
        double BmsDemandVoltage = 220;



        public OutputVoltageErr_FeedBackLoad(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            BmsDemandVoltage = MaxAllowChargeVoltage - 100;
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
                                //测试时间|输入电压|输出电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                //开始检测流程
                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BmsDemandVoltage, 250, true, BmsDemandVoltage);
                Thread.Sleep(500);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BmsDemandVoltage, LstChargerInfo[0].ChargerType, 0);
                //检查刷卡上电
                double Voltage = 0;
                if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                {
                    Voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingVoltage;
                }
                if (Voltage < 50)
                {
                    SendNoticeToUIAndTxtFile("未检测到" + lstIDs[0] + "号枪处于充电状态，该枪停止检测");
                    return;
                }
                //if (!CheckSwipingCard(testWorkParam.lstIDs, OutputVoltage - 50))
                //{
                //    return;
                //}

                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, OutputVoltage - 50, 120, true, OutputVoltage - 50);
                //Thread.Sleep(3000);



                //ControlEquipMent.FeedbackLoad?.SetFeedbackLoadParams(testWorkParam.lstIDs, OutputVoltage - 50 - 20, 20);
                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                {
                    SendNoticeToUIAndTxtFile(string.Format("调整需求电压为{0}V", BmsDemandVoltage));
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, 480, 20, 500, 20);
                }
                Thread.Sleep(10000);    //等待负载稳定
                d1 = new Dictionary<int, string>();
                //d2 = new Dictionary<int, string>();
                foreach(int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                    //d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString());
                }
                ProcessDataTmp(d1, "过压保护前正常充电", "充电电压(V)", "-", "-");
                //ProcessDataTmp(d2, "过压保护前正常充电", "充电电流(A)", "-", "-");

                //ControlEquipMent.FeedbackLoad?.FeedbackLoad_ON(testWorkParam.lstIDs);
                //惠州TB新更新的BMS版本可以设置超过1200V的电压，所以这里不需要通过回馈负载来模拟过压（后续应该用版本号来区分）
                if (!Customer.Equals("TB") && (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null))
                {
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定

                    SendNoticeToUIAndTxtFile("发送输出电压异常值：" + OutputVoltage + "V，等待输出稳定。");
                    SetLoadPara(testWorkParam.lstIDs, OutputVoltage, 20, OutputVoltage, 20);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime(testWorkParam.lstIDs, 20, 10);

                }
                else
                {
                    SendNoticeToUIAndTxtFile("模拟输出过压");
                    List<bool> Ks = GetKStatus16_Charging_DC();

                    Ks[26] = true;
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, OutputVoltage);
                    Thread.Sleep(300);
                    ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, OutputVoltage, Ks.ToArray());
                    Thread.Sleep(1000);
                }
                //ControlEquipMent.FeedbackLoad?.SetFeedbackLoadParams(testWorkParam.lstIDs, OutputVoltage, 20);
                //输出的过压可能到不了预设值，所以这里读取一段时间内的最大值作为输出值
                d1 = new Dictionary<int, string>();
                foreach(int item in testWorkParam.lstIDs)
                {
                    double outputVolt = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    int timeout = 50;
                    while(timeout-- > 0)
                    {
                        double outputVolt1 = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        if (outputVolt1 > outputVolt)
                        {
                            outputVolt = outputVolt1;
                        }
                        Thread.Sleep(100);
                    }
                    d1.Add(item, outputVolt.ToString("F2"));
                }
                ProcessDataTmp(d1, "模拟过压时", "直流输出电压(V)", "-", "-");

                Thread.Sleep(2000);
                //var Data_Volt = new Dictionary<int, string>();
                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                //    int timeout = 100;
                //    while (timeout-- > 0)
                //    {
                //        if (voltage < 30)
                //        {
                //            Thread.Sleep(100);
                //            voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                //            if (voltage < 30)
                //                break;
                //        }
                //        voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                //        Thread.Sleep(100);
                //    }
                //    Data_Volt.Add(testWorkParam.lstIDs[i], voltage.ToString());
                //}
                //ProcessDataTmp(Data_Volt, "保护后桩应停止充电", "模拟过压后输出电压(V)", "-", "-");

                //CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有告警提示", 999, 2);
                //ProcessDataConnect("应发出告警提示", "是否有告警提示");


                Thread.Sleep(1000 * 10);
                // 关闭回馈负载
                if (ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null)
                {
                    SendNoticeToUIAndTxtFile("恢复正常电压：" + (BmsDemandVoltage - 20) + "V，等待直流输出稳定。");
                    SetLoadPara(testWorkParam.lstIDs, BmsDemandVoltage - 20, 20, OutputVoltage - 50, 20);

                    Thread.Sleep(500);
                    SetLoadDCOFF(testWorkParam.lstIDs);
                }
                // 断开过压开关
                List<bool> Ks1 = GetKStatus16_Charging_DC();
                Ks1[26] = false;
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(3000);
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks1.ToArray());

                //WaitDCVoltage(testWorkParam.lstIDs, BmsDemandVoltage);
                Thread.Sleep(1000 * 5);

                //设置测试条件
                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //        d1.Add(testWorkParam.lstIDs[i], BmsDemandVoltage.ToString());
                //        d2.Add(testWorkParam.lstIDs[i], OutputVoltage.ToString());
                //}
                //SetConditionValue("BMS需求电压(V)", d1);
                //SetConditionValue("过压参考值(V)", d2);
                SetConditionValues();

                //采集数据
                //ProcessDataResult(testWorkParam.lstIDs, BmsDemandVoltage.ToString(), "BMS需求电压(V)", true, "");
                //ProcessDataResult(testWorkParam.lstIDs, OutputVoltage.ToString(), "过压参考值(V)", true);
                var Data_Volt = new Dictionary<int, string>();
                //var Data_Current = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                    int timeout = 20;
                    while(timeout-- > 0)
                    {
                        if(voltage < 30)
                        {
                            break;
                        }
                        voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                        Thread.Sleep(100);
                    }
                    Data_Volt.Add(testWorkParam.lstIDs[i], voltage.ToString());
                }
                //ProcessDataTmp(Data_Tmp, "输出电压异常", "保护后桩输出电压(V)", "0", "30");
                ProcessDataTmp(Data_Volt, "保护后桩应停止充电", "保护后桩输出电压(V)", "0", "50");
                //这里不能采集电压，因为我们自己会有模拟的过压电压，所以只需要判断充电状态即可。
                string chargerState = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[0]].ChargingState;
                if (chargerState != "充电中")
                {
                    ProcessDataResult(testWorkParam.lstIDs, "是", "是否切断直流输出", true, "保护后桩应停止充电");
                    //Thread.Sleep(1000);
                    //ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", true, "保护后桩应停止充电");
                }
                else
                {
                    ProcessDataResult(testWorkParam.lstIDs, "否", "是否切断直流输出", false, "保护后桩应停止充电");
                    //Thread.Sleep(1000);
                    //ProcessDataResult(testWorkParam.lstIDs, chargerState, "充电状态", false, "保护后桩应停止充电");
                }
                //ProcessDataTmp(Data_Current, "保护后桩应停止充电", "充电电流(A)", "0", "5");

                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有告警提示", 999, 2);
                ProcessDataConnect("保护后桩应发出告警", "是否有告警提示");

                //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                //Thread.Sleep(1000);
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


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;
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
                    //测试时间|BMS需求电压|输出电压|是否停机|测试结果     
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + BmsDemandVoltage + "|" + OutputVoltage + "|" + strResult;
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
