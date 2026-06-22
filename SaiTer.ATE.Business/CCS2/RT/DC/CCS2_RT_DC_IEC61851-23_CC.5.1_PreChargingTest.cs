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
    /// 欧标研测直流：预充电
    /// </summary>
    public class CCS2_RT_DC_PreChargingTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public CCS2_RT_DC_PreChargingTest(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            //string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPRersh_EUDCALL();
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
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();

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
                ////是否全部有结论
                //if (testWorkParam.lstIDs.Count <= 0) break;
                ////是否超时
                //if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
                //{
                //    for (int i = 0; i < LstTrialData.Count; i++)
                //    {
                //        if (LstTrialData[i].IsCheck)
                //        {
                //            if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                //            {
                //                LstTrialData[i].TrialResult = EmTrialResult.Fail;
                //                LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                //                int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                //                LstTrialData[i].PKID = LstChargerInfo[k].PKID;
                //                //界面展示的数据项格式
                //                //
                //                LstTrialData[i].ExtentData = "-|-|-|-|null";
                //                SendTrialDataToUI(LstTrialData[i]);
                //            }
                //        }
                //    }
                //    break;
                //}

                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                //添加测试条件
                SetConditionValues();
                //d1 = new Dictionary<int, string>();
                //foreach (int item in testWorkParam.lstIDs)
                //{
                //    d1.Add(item, LstChargerInfo[0].NominalVoltage.ToString("F2"));
                //}
                //SetConditionValue("BMS需求电压(V)", d1);

                int waitTime = 50;
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_VVoltage", "1", "V", false, "250", "-2");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "A", false, "10", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                Thread.Sleep(1000);
                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 1);//最大值
                Thread.Sleep(waitTime);
                //设置时基
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "50", "0");
                Thread.Sleep(waitTime);
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(1000);
                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, (MaxAllowChargeVoltage * 0.95).ToString(), "Single");
                Thread.Sleep(waitTime);


                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, Ks.ToArray(), 0, 0);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, BMSDemandVolt, 250);
                Thread.Sleep(200);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, true, BMSDemandVolt);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                int timeout = 300;
                //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                //MessgaeInfo(true, "请刷卡充电!", true);
                //while (timeout-- > 0)
                //{
                //    var bmsData = AllEquipStateData.DicBMS_EU_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                //    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                //        continue;
                //    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus_EU_DC(c.Value.SystemState) >= 20);
                //    if (ALLCanCharge)
                //    {
                //        break;
                //    }

                //    System.Threading.Thread.Sleep(1000);
                //}
                //MessgaeInfo(false, "请刷卡充电!");
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile("回读示波器数据");

                Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);
                ProcessDataTmp(Data_Tmp, "初始化阶段电压检查", "预充电压(V)", (MaxAllowChargeVoltage * 0.95).ToString("F2"), (MaxAllowChargeVoltage * 1.05).ToString("F2"), dImgs);


                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
