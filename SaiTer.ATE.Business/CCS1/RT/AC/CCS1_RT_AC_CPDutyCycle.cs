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
    public class CCS1_RT_AC_CPDutyCycle : BusinessBase
    {
        double PwmMax = 0;
        double PwmMin = 0;
        Dictionary<int, string> DicPwmOne = new Dictionary<int, string>();//第一个占空比检测点的数据结果  <枪位号，结果数据>
        Dictionary<int, string> DicPwmTwo = new Dictionary<int, string>();
        Dictionary<int, string> DicPwmThree = new Dictionary<int, string>();
        Dictionary<int, string> dicPath = new Dictionary<int, string>();//截图保存路径
        int TrialFlowNum = 1;

        public CCS1_RT_AC_CPDutyCycle(int trialType) { TrialType = trialType; }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length > 1)
            {
                PwmMax = Convert.ToDouble(strParams[1].Split('=')[1]);
                PwmMin = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            else
            {
                double dutyCycle = RatedCurrent / 0.6;
                if (dutyCycle <= 20 && dutyCycle >= 10)
                {
                    PwmMin = 10;
                    PwmMax = 20;
                }
                else if (dutyCycle > 20 && dutyCycle <= 85)
                {
                    PwmMin = 20;
                    PwmMax = 85;
                }
                else if (dutyCycle > 85)
                {
                    dutyCycle = RatedCurrent / 2.5 + 64;
                    if (dutyCycle > 85 && dutyCycle <= 96)
                    {
                        PwmMin = 85;
                        PwmMax = 96;
                    }
                }
            }
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
                                LstTrialData[i].ExtentData = "-|-|-|-|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                if (PwmMin < 10 || PwmMax > 96)
                {
                    ProcessDataResult(testWorkParam.lstIDs, "充电桩最大电流不符合", "-", false, "CP占空比试验");
                    break;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }
                if (testWorkParam.lstIDs.Count > 0)
                {
                    SendNoticeToUIAndTxtFile("关闭示波器1、2、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    SendNoticeToUIAndTxtFile("开启示波器3号通道，并设置相应参数");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CPPwm", "50", "V", false, "10", "0");

                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "100", 3, "3V", "Single");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    SendNoticeToUIAndTxtFile("正在检测插枪状态");
                    WaitSwipingCard(testWorkParam.lstIDs, 1);

                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CCResistance < 1.23
                           && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CCResistance > 1.82)
                        {
                            SendNoticeToUIAndTxtFile("未检测到" + testWorkParam.lstIDs[i] + "号枪插枪状态，该枪停止检测");
                            testWorkParam.lstIDs.Remove(testWorkParam.lstIDs[i]);
                        }
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        SendNoticeToUIAndTxtFile("所有枪位都未检测到插枪状态，测试结束！");
                        return;
                    }

                    //if (!AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].SystemState.Contains("充电中"))
                    //{
                    //    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    //    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    //}
                    CheckSwipingCard(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    SendNoticeToUIAndTxtFile("设置示波器3号通道测量项为【CP占空比】");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    TrialFlowNum = 1;

                    Thread.Sleep(200);
                    SendNoticeToUIAndTxtFile("开始读示波器第1个CP占空比测量点");
                    dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    DicPwmOne = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    Thread.Sleep(100);
                    ProcessDataTmp(DicPwmOne, "测量点1", "CP占空比(%)", PwmMin.ToString(), PwmMax.ToString(), dicPath);


                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    SendNoticeToUIAndTxtFile("开始读示波器第2个CP占空比测量点");
                    Thread.Sleep(4800);
                    TrialFlowNum = 2;
                    dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    DicPwmTwo = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    ProcessDataTmp(DicPwmTwo, "测量点2", "CP占空比(%)", PwmMin.ToString(), PwmMax.ToString(), dicPath);


                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    SendNoticeToUIAndTxtFile("开始读示波器第3个CP占空比测量点");
                    Thread.Sleep(5000);
                    TrialFlowNum = 3;
                    dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    DicPwmThree = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    ProcessDataTmp(DicPwmThree, "测量点3", "CP占空比(%)", PwmMin.ToString(), PwmMax.ToString(), dicPath);

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


                    LstTrialData[k].TrialName = TrialItem.ItemName;

                    switch (TrialFlowNum)
                    {
                        case 1:
                            LstTrialData[k].Data1 = DicPwmOne[LstChargerInfo[i].ChargerId];
                            LstTrialData[k].ItemName = "测量点" + TrialFlowNum;
                            LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicPwmOne[LstChargerInfo[i].ChargerId] + "|" + PwmMin + "|" + PwmMax + "|" + dicPath[LstChargerInfo[i].ChargerId];
                            if (!string.IsNullOrEmpty(DicPwmOne[LstChargerInfo[i].ChargerId]) && DicPwmOne[LstChargerInfo[i].ChargerId] != "null")
                            {
                                if (Convert.ToDouble(DicPwmOne[LstChargerInfo[i].ChargerId]) <= PwmMax && Convert.ToDouble(DicPwmOne[LstChargerInfo[i].ChargerId]) >= PwmMin)
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            }
                            break;
                        case 2:
                            LstTrialData[k].Data1 = DicPwmTwo[LstChargerInfo[i].ChargerId];
                            LstTrialData[k].ItemName = "测量点" + TrialFlowNum;
                            LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicPwmTwo[LstChargerInfo[i].ChargerId] + "|" + PwmMin + "|" + PwmMax + "|" + dicPath[LstChargerInfo[i].ChargerId];
                            if (!string.IsNullOrEmpty(DicPwmTwo[LstChargerInfo[i].ChargerId]) && DicPwmTwo[LstChargerInfo[i].ChargerId] != "null")
                            {
                                if (Convert.ToDouble(DicPwmTwo[LstChargerInfo[i].ChargerId]) <= PwmMax && Convert.ToDouble(DicPwmTwo[LstChargerInfo[i].ChargerId]) >= PwmMin)
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            }
                            break;
                        case 3:
                            LstTrialData[k].Data1 = DicPwmThree[LstChargerInfo[i].ChargerId];
                            LstTrialData[k].ItemName = "测量点" + TrialFlowNum;
                            LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicPwmThree[LstChargerInfo[i].ChargerId] + "|" + PwmMin + "|" + PwmMax + "|" + dicPath[LstChargerInfo[i].ChargerId];

                            if (!string.IsNullOrEmpty(DicPwmThree[LstChargerInfo[i].ChargerId]) && DicPwmThree[LstChargerInfo[i].ChargerId] != "null")
                            {
                                if (Convert.ToDouble(DicPwmThree[LstChargerInfo[i].ChargerId]) <= PwmMax && Convert.ToDouble(DicPwmThree[LstChargerInfo[i].ChargerId]) >= PwmMin)
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            }
                            break;
                            //case 4:
                            //    //测量点序号|频率测量值(Hz)|频率下限|频率上限|测试结果|查看示波器截图 
                            //    LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicPwmOne[LstChargerInfo[i].ChargerId] + "|" + DicPwmTwo[LstChargerInfo[i].ChargerId] + "|" + DicPwmThree[LstChargerInfo[i].ChargerId] + "|" + PwmMin + "|" + PwmMax + "|" + dicPath[LstChargerInfo[i].ChargerId];
                            //    break;
                    }
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
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
