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
    /// 国标产测直流：电流纹波因数试验
    /// </summary>
    public class GB_PT_DC_CurrentRippleFactor : BusinessBase
    {
        int trlTimeOut_S = 30;
        double CurrentPKPK = 12;//电流纹波峰峰值最大值
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储


        public GB_PT_DC_CurrentRippleFactor(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            dImgs = new Dictionary<int, string>();//图片存储
            //电流纹波峰峰值最大值 = 12
            string[] strParams = TrialItem.ResultParams.Split('|');
            CurrentPKPK = Convert.ToDouble(strParams[0].Split('=')[1]);
        }
        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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
                    SendNoticeToUIAndTxtFile("开启并机继电器");
                    CombineControlResistance();

                    SendNoticeToUIAndTxtFile("开启负载并机");

                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);


                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "2000000");
                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道，切换交流耦合");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_DC_Ripple", "1", "V", false, "1", "-2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "AC", "20", Channel2, "Output_DC_I", "1", "A", false, "5", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "100", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", Channel4, "Input_AC_V", "1", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);
                    Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 1);//
                    //Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "PKPK", 2);//
                    Thread.Sleep(waitTime);
                    //设置时基ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0");

                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 1);//
                    Thread.Sleep(waitTime);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    SetConditionValues();

                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Auto");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    CurrentRipple(MaxAllowChargeVoltage.ToString(), RatedCurrent.ToString(), 0, $"额定输出电流{RatedCurrent}(A)");

                    double tVoltageSet = RatedMinVoltage > MaxAllowChargeVoltage ? MaxAllowChargeVoltage : RatedMinVoltage;
                    CheckSwipingCard(testWorkParam.lstIDs);
                    ////////////////////////////////
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Auto");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    CurrentRipple(tVoltageSet.ToString(), MaxAllowChargeCurrent.ToString(), 1, $"最大输出电流{MaxAllowChargeCurrent}(A)");



                    SendNoticeToUIAndTxtFile("关闭并机继电器");
                    SingleControlResistance();

                    SendNoticeToUIAndTxtFile("取消负载并机");

                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }




        public void CurrentRipple(string tVoltageSet, string tCurrentSet, int tCount, string state)//电流纹波
        {
            try
            {
                if (IsRLoad(Convert.ToDouble(tVoltageSet), Convert.ToDouble(tCurrentSet)))
                {


                    Double voltageSet = System.Math.Abs(Convert.ToDouble(tVoltageSet));
                    Double currentSet = System.Math.Abs(Convert.ToDouble(tCurrentSet));//注意：取绝对值做比较


                    int index = tCount + 1;
                    SendNoticeToUIAndTxtFile("正在测试第" + index + "个点");

                    if (tCount == 0)
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltageSet, currentSet, false, voltageSet);
                        WaitDCVoltage(lstIDs, voltageSet);
                    }
                    else
                    {
                        ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, voltageSet + 20, currentSet, false, voltageSet);
                        WaitDCVoltage(lstIDs, voltageSet + 20);
                    }

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载稳定");
                    if (tCount == 0)
                    {
                        SetLoadPara(testWorkParam.lstIDs, voltageSet - 10, currentSet + 20, voltageSet - 5, currentSet);
                        Thread.Sleep(300);
                        SetLoadDCON(testWorkParam.lstIDs);

                    }
                    else
                    {
                        SetLoadPara(testWorkParam.lstIDs, voltageSet, currentSet + 20, voltageSet, currentSet);
                        Thread.Sleep(300);
                        SetLoadDCON(testWorkParam.lstIDs);
                    }
                    int timeout = 35;
                    double Current = 0;
                    while (timeout-- > 0)
                    {
                        Current = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                        if (Current > currentSet * 0.9)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    double CurrentSet = currentSet;
                    Current = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(testWorkParam.lstIDs[0], Current.ToString());
                    // ProcessDataTmp(dic, "预置条件", "带载状态", (CurrentSet * 0.8).ToString("F2"), (CurrentSet * 1.1).ToString("F2"));
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("读示波器交流电压峰峰值");
                    dic.Clear();
                    dic = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 2);//交流电压峰峰值
                    int count = 10;
                    while (count-- > 0)
                    {
                        if (dic == null || dic.Count == 0 || Convert.ToDouble(dic[1]) == 0 || Convert.ToDouble(dic[1]) > CurrentPKPK)
                        {
                            dic = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "PKPK", 2);//交流电压峰峰值
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            break;
                        }
                    }

                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    d1 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                        timeout = 50;
                        while (timeout-- > 0)
                        {
                            if (current < currentSet * 0.9 || current > currentSet * 1.1)
                            {
                                current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                                Thread.Sleep(100);
                                continue;
                            }
                            break;
                        }
                        d1.Add(item, current.ToString("F2"));
                    }
                    ProcessDataTmp(d1, state, "直流输出电流(A)", (currentSet * 0.9).ToString("F2"), (currentSet * 1.1).ToString("F2"));
                    ProcessDataTmp(dic, state, "输出电流纹波峰峰值(A)", "0", CurrentPKPK.ToString(), dImgs);

                    SetLoadDCOFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }



    }
}
