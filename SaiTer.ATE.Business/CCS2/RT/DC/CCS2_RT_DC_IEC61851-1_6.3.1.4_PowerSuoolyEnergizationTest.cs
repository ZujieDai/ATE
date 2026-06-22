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
    /// 欧标研测直流：给电动汽车充电（测CP断线后的CP电压）
    /// </summary>
    public class CCS2_RT_DC_PowerSuoolyEnergizationTest : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        string itemFlow = "";
        public CCS2_RT_DC_PowerSuoolyEnergizationTest(int type) { TrialType = type; }

        public override void InitializeParams()
        {
        }

        public override void InitEquiMent()
        {

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
                var Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");
                Thread.Sleep(500);
                ControlEquipMent.BMS.BMS_OFF(lstIDs);

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
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
                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }

                    //设置测试条件
                    SetConditionValues();

                    //升源，启动BMS
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        if (AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt < 50)
                        {
                            ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                            //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                        }
                    }

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ////检测刷卡
                    //WaitSwipingCard(testWorkParam.lstIDs, 0);


                    if (testWorkParam.lstIDs.Count > 0)
                    {

                        //初始化示波器
                        SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                        ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.5");
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                        //设置时基400ms
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1.5");//低值
                        Thread.Sleep(100);
                        SendNoticeToUIAndTxtFile("添加示波器测量项");
                        //添加测量值
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                        Thread.Sleep(100);
                        ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                        Thread.Sleep(100);

                        #region CP断线
                        //设置触发
                        ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "10.8", "Single");//上升边沿触发，自动
                        Thread.Sleep(100);

                        //启动示波器
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                        Thread.Sleep(3500);

                        SendNoticeToUIAndTxtFile("模拟导引CP断线");
                        //模拟CP断线
                        bool[] Ks = new bool[24];
                        Ks[0] = true;//DC+DC-控制
                        Ks[1] = true;//CC信号控制
                        Ks[2] = false;//CP信号控制
                        Ks[4] = true;//PE信号控制
                        ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");

                        Thread.Sleep(2000);
                        SendNoticeToUIAndTxtFile("正在分析示波器数据");
                        //读取分析数据
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                        CPPWMUpTime(testWorkParam.lstIDs, 3, 10.8, 1);//CP动作时间
                        ACDownTime(testWorkParam.lstIDs, 1, 30, 2);//AC回路动作时间

                        //读取卡点时间
                        ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                        Data_Tmp = GetOSCTime(OscTime_Tmp);
                        Dictionary<int, string> dd = new Dictionary<int, string>();
                        foreach (var itmp in Data_Tmp)
                        {
                            dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                        }
                        dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                        ProcessDataTmp(dd, "CP断线", "电源切断时间(ms)", "0", "100", dImgs);

                        //断线后的电压
                        Data_Tmp = new Dictionary<int, string>();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString());
                        }
                        ProcessDataTmp(Data_Tmp, "CP断线", "断线后输出电压(V)", "0", "20");

                        Data_Tmp = new Dictionary<int, string>();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                        }
                        ProcessDataTmp(Data_Tmp, "CP断线", "断线后CP电压(V)", "11.4", "12.6");
                        #endregion
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
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
                LstTrialData[k].ItemName = iIndex.ToString();

                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = $"{itemFlow}|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
            iIndex++;
        }
    }
}
