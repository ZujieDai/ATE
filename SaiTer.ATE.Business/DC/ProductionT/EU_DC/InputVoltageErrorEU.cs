using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 过压欠压欧标
    /// </summary>
    class InputVoltageErrorEU : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        double InputVoltage = 220;
        double BMSVoltage = 500; 
        private double BMSMeasureVoltage = 390;//过压参考值
        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public InputVoltageErrorEU(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            BMSVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);

        }
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
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
                SetCPRersh_EUDCALL();
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
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + 220 + "|" + InputVoltage + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                SetConditionValues();


                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs,BMSVoltage, 10,true,BMSMeasureVoltage);
                //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //Thread.Sleep(10 * 1000);
                //}

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

                    ////闭合开关S2，启动充电
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, BMSVoltage, LstChargerInfo[0].ChargerType, 0);



                    Thread.Sleep(2000);

                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, InputVoltage);
                    //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");
                    Thread.Sleep(1000 * 16);

                    ////设置测试条件
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    int key = testWorkParam.lstIDs[i];
                    //    if (AllEquipStateData.DicACSource_StateData.Count == 1)
                    //    {
                    //        key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                    //    }
                    //    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                    //    d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    //}
                    //SetConditionValue("供电电压(V)", d1);
                    //SetConditionValue("供电频率(Hz)", d2);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double? voltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        for (int j = 0; j < 20; j++)
                        {
                            if (voltage > 20)
                            {
                                voltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;

                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
                        }

                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(Data_Tmp, "输入电压异常", "保护后桩输出电压(V)", "0", "20");

                    SendNoticeToUIAndTxtFile("恢复正常电压：" + 220 + "V，等待交流源输出稳定。");
                    ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, 220);
                    //ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                    ////这个是有恢复测试的
                    //if (TrialType == (int)EmTrialType.输入过压保护及恢复测试
                    //    || TrialType == (int)EmTrialType.输入欠压保护及恢复测试)
                    //{
                    //    Thread.Sleep(5000);

                    //    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //    {
                    //        //return;
                    //    }

                    //    Data_Tmp = new Dictionary<int, string>();
                    //    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //    {
                    //        double voltage = 0;
                    //        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC ||
                    //                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                    //                LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                    //        {
                    //            voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                    //            for (int j = 0; j < 20; j++)
                    //            {
                    //                if (voltage < 80)
                    //                {
                    //                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                    //                    Thread.Sleep(1000);
                    //                }
                    //                else
                    //                {
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                    //            LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                    //            LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                    //        {
                    //            voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    //            //voltage = 0;
                    //            for (int j = 0; j < 20; j++)
                    //            {
                    //                if (voltage < 80)
                    //                {
                    //                    voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    //                    Thread.Sleep(1000);
                    //                }
                    //                else
                    //                {
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                    //    }
                    //    ProcessDataTmp(Data_Tmp, "输入电压正常", "恢复后桩输出电压(V)", "80", "260");

                    //    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    //}


                    //Thread.Sleep(2000);
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
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
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
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + BMSVoltage + "|" + InputVoltage + "|" + strResult;
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
