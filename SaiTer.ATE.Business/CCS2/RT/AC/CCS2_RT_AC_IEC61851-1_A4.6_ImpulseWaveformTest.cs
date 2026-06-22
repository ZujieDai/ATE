using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标研测交流：脉冲波形测试
    /// </summary>
    public class CCS2_RT_AC_ImpulseWaveformTest : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储

        public CCS2_RT_AC_ImpulseWaveformTest(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {

        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPReresh();
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);


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
                                //测试时间|电压|电流|CP电压|CP频率|CP占空比
                                LstTrialData[i].ExtentData = DateTime.Now.ToString()
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + "";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                //设置测试条件
                SetConditionValues();

                try
                {
                    int waitTime = 50;

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.5", "0");
                    Thread.Sleep(waitTime);

                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    Thread.Sleep(waitTime);

                    //闭合CP,断开S2
                    var Ks = GetKStatus16_Charging();
                    Ks[0] = false;
                    Ks[3] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    //检测是否刷卡（有PWM波即可）
                    WaitSwipingCard(testWorkParam.lstIDs, 2);
                    Thread.Sleep(3000);

                    //上升沿下降沿触发闭合，关闭滤波功能
                    List<bool> lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    lstConditionState[15] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    Thread.Sleep(300);

                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    ProcessDataTmp(Data_Tmp, "B2状态", "CP电压(V)", "8.37", "9.59", dImgs);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    //ProcessDataTmp(dd, "B2状态", "CP上升时间(us)", "-", "2");
                    ProcessDataTmp(dd, "B2状态", "CP上升时间(us)", "0", "10");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    ProcessDataTmp(dd, "B2状态", "CP下降时间(us)", "0", "13");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    ProcessDataTmp(Data_Tmp, "B2状态", "CP占空比", "10", "90");

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //恢复滤波功能
                    //lstConditionState[15] = false;
                    //ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    //Thread.Sleep(300);

                    SendNoticeToUIAndTxtFile("启动示波器");
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                    //上升沿下降沿触发闭合，关闭滤波功能
                    //lstConditionState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    //lstConditionState[15] = true;
                    //ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                    //Thread.Sleep(300);

                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    ProcessDataTmp(Data_Tmp, "C2状态", "CP正电压(V)", "5.47", "6.53", dImgs);

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    ProcessDataTmp(dd, "C2状态", "CP上升时间(us)", "0", "7");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                    }
                    ProcessDataTmp(dd, "C2状态", "CP下降时间(us)", "0", "13");

                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);
                    ProcessDataTmp(Data_Tmp, "C2状态", "CP占空比", "10", "90");

                    //恢复滤波功能
                    lstConditionState[15] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstConditionState);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            }
        }

        public override void ProcessData()
        {

        }

    }
}
