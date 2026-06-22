using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class GB_RT_DC_PreCharging : BusinessBase
    {
        double BMSDemandVolt = 0;
        float trlTimeOut_S = 100;//超时时间

        public GB_RT_DC_PreCharging(int type) { TrialType = type; }

        public override void InitializeParams()
        {
            Init();
            //BMS需求电压(V)=500|电池电压(V)=390
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 1 && strParams[0].Split('=').Length >= 2)
            {
                BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            if (strParams.Length >= 2)
            {
                BatteryVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
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
                SetCPReresh();
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
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
                                //
                                LstTrialData[i].ExtentData = "-|-|-|-|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                //ControlEquipMent.BMS.SetParams(testWorkParam.lstIDs, BMSDemandVolt, LoadCurrent + 10, true);

                //ControlEquipMent.BMS.BMS_ON(lstIDs);
                double offset = -3.4;
                double gear = 150;
                int waitTime = 50;
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, gear.ToString(), offset.ToString());
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "K1K2_Volt", "50", "V", false, gear.ToString(), offset.ToString());
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", "1", "K1K2_Sign", "50", "V", false, "5", "-3.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "Input_AC_V", "50", "V", false, "10", "1");
                Thread.Sleep(waitTime);

                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 1);
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 2);
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "Y");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "-0.8");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(2000);

                var K1K2s2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);

                SendNoticeToUIAndTxtFile("开启导引中");
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, BatteryVoltage);   //互操蓄电池电压
                Thread.Sleep(300);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BatteryVoltage, MaxAllowChargeVoltage, 250);   //BCP报文电池电压
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, RatedCurrent, false, RatedCurrent);
                Thread.Sleep(300);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(300);


                MessgaeInfo(true, "请刷卡充电!");
                int timeout = 90 * 5;
                while (timeout-- > 0)
                {
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                    if (state >= 5 /*&& state <= 7*/)
                    {
                        SendNoticeToUIAndTxtFile("下发示波器触发。");
                        foreach (var item in K1K2s2)
                        {
                            if (Convert.ToDouble(item.Value) < 2)
                            {
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(new List<int>() { item.Key }, 0, "RISE", "DC", "EDGE", "0", 3, "6", "Single");
                            }
                            else
                            {
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(new List<int>() { item.Key }, 0, "FALL", "DC", "EDGE", "0", 3, "6", "Single");
                            }
                        }
                        break;
                    }
                    if (timeout % 5 == 0)
                    {
                        int residuetime = timeout / 5;
                        SendNoticeToUIAndTxtFile("刷卡剩余倒计时:" + residuetime);
                    }
                    System.Threading.Thread.Sleep(200);
                }

                MessgaeInfo(false, "请刷卡充电!");

                timeout = 20 * 5;
                while (timeout-- > 0)
                {

                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                    if (state == 9)
                    {
                        //ControlEquipMent.Oscillograph?.Oscillograph_TriggerTypeSet("Single");
                        break;
                    }
                    if (timeout % 5 == 0)
                    {
                        int residuetime = timeout / 5;
                        SendNoticeToUIAndTxtFile("等到到达充电中:" + residuetime);
                    }
                    System.Threading.Thread.Sleep(200);

                }

                SetConditionValues();

                Thread.Sleep(2000);

                //获取触发位的实际值
                var dicOutVolt = GetPosistionValue(testWorkParam.lstIDs, 1, 0.8);
                var dicK1K2Volt = GetPosistionValue(testWorkParam.lstIDs, 2, 0.8);
                //计算Y轴位置
                foreach (int chargerId in testWorkParam.lstIDs)
                {
                    double y1 = 0;
                    double y2 = 0;
                    if (dicOutVolt.ContainsKey(chargerId) && dicK1K2Volt.ContainsKey(chargerId))
                    {
                        y1 = Math.Round(Convert.ToDouble(dicOutVolt[chargerId]), 2);
                        y2 = Math.Round(Convert.ToDouble(dicK1K2Volt[chargerId]), 2);
                        dicOutVolt[chargerId] = y1.ToString();
                        dicK1K2Volt[chargerId] = y2.ToString();
                        ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_Y(new List<int> { chargerId }, 1, y1, 1);
                        ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_Y(new List<int> { chargerId }, 1, y2, 2);
                    }
                }

                var dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                var OscTime_Tmp = new Dictionary<int, double[]>();
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                var Data_Tmp = GetOSCY(OscTime_Tmp);
                //Dictionary<int, string> dic = new Dictionary<int, string>();
                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    string strVolt = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString();
                //    dic.Add(testWorkParam.lstIDs[i], strVolt);
                //}
                ProcessDataTmp(dicOutVolt, "闭合K5和K6后", "电池电压光标值(Y1)", "-", "-");
                ProcessDataTmp(dicK1K2Volt, "闭合K5和K6后", "K1K2前端电压光标值(Y2)", " - ", "-");
                ProcessDataTmp(Data_Tmp, "闭合K5和K6后", "Y1和Y2差值", " 1 ", "10", dImgs);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
            }

        }


        public override void ProcessData()
        {

        }
    }
}
