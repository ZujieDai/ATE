using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流  输入电流过冲
    /// </summary>
    public class InputCurrentOvershoot_GB_DC222 : BusinessBase
    {

        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        double errorPecent = 10;//误差百分比
        public InputCurrentOvershoot_GB_DC222(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            errorPecent = Convert.ToDouble(strParams[0].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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
                    SendNoticeToUIAndTxtFile("负载并机");
                    ControlEquipMent.FeedbackLoad.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    if (ControlEquipMent.ResistanceLoad != null)
                    {
                        ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);
                    }
                    //此条测试项目关闭回馈负载缓起功能，其它测试项目默认打开缓起模式
                    //回馈负载如果不设置，缓起功能默认开状态
                    //暂没找到协议缓起开关 




                    int waitTime = 50;

                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "Input_AC_V", "1", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_DC_V", "1", "V", false, "300", "0");//通道1设置2
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_DC_I", "1", "A", false, "50", "-2");//通道2设置 惠州TB200：1
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "100", "0");//通道3设置 惠州TB100：1


                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "2M");
                    //设置时基ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "50", "0");
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 3);//
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 3);//
                    Thread.Sleep(waitTime);




                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    Dictionary<int, string> dicInCurrent = new Dictionary<int, string>();
                    Dictionary<int, string> dicInACImax = new Dictionary<int, string>();
                    Dictionary<int, string> dicInACImax2 = new Dictionary<int, string>();
                    Dictionary<int, string> dicInACTOP = new Dictionary<int, string>();

                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 1, BMSDemandVolt, 1);

                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, 0, true, BMSDemandVolt);

                    SendNoticeToUIAndTxtFile("设备开启负载中，请稍候...");

                    
                   SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 70, BMSDemandVolt, 70);
                    Thread.Sleep(300);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "60", "Single");//上升边沿触发
                    Thread.Sleep(300);
                    Dictionary<int, bool> dicTemp = new Dictionary<int, bool>();
                    dicTemp = ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs);
                    int count = 5;
                    while (count-- > 0)
                    {
                        if (dicTemp[1])
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "60", "Single");//上升边沿触发
                           
                            Thread.Sleep(500);
                        }
                        else
                        { break; }

                    }

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, 60, false, BMSDemandVolt);
                    Thread.Sleep(1000 * 15);

                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();
                    //double current = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    //while (sw.ElapsedMilliseconds / 1000 > 30)
                    //{
                    //    if (current < (MaxAllowChargeCurrent - 20) * 0.95)
                    //    {
                    //        current = AllEquipStateData.DicPowerAnalyzer_StateData[LstChargerInfo[0].ChargerId].Channel4RMSCurrent;
                    //        Thread.Sleep(200);
                    //    }
                    //    else
                    //    {
                    //        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    //        break;
                    //    }
                    //}

                    dicInACImax = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 3); //采集的是输入交流电流，最大值
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dicInACImax, BMSDemandVolt + "V,60A", "启动输入电流最大值", "-", "-", dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(2000);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "60", "Single");//上升边沿触发

                    dicInCurrent = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RMS", 3); //采集的是输入交流电流，最大值
                    ProcessDataTmp(dicInCurrent, BMSDemandVolt + "V,60A", "额定输入电流均方根", "-", "-");
                    dicInACTOP = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3); //采集的是额定输入电流峰值
                    ProcessDataTmp(dicInACTOP, BMSDemandVolt + "V,60A", "额定输入电流顶端值", "-", "-");
                    dicInACImax2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 3); //采集的是输入交流电流，最大值
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dicInACImax2, BMSDemandVolt + "V,60A", "稳定输入电流最大值", "-", "-", dImgs);


                    double InCurrentIppSet = Convert.ToDouble(dicInCurrent[1]) * 1.414213562373095;

                    Double ErrorValue = 0;


                    Double ErrorValue1 = System.Math.Abs(Convert.ToDouble(dicInACImax[1]) * 100 / InCurrentIppSet); //误差   最大值==峰值
                    Double ErrorValue2 = System.Math.Abs(Convert.ToDouble(dicInACImax[1]) * 100 / Convert.ToDouble(dicInACImax2[1]));
                    ErrorValue = ErrorValue1 > ErrorValue2 ? ErrorValue2 : ErrorValue1;

                    Dictionary<int, string> dicResultData = new Dictionary<int, string>();
                    dicResultData.Add(1, ErrorValue.ToString("F2"));

                    ProcessDataTmp(dicResultData, "电流过冲", "启动电流MAX/稳定电流MAX(%)", (100 - errorPecent).ToString("F2"), (100 + errorPecent).ToString("F2"));


                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(200);
                    SendNoticeToUIAndTxtFile("负载取消并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);

                    Thread.Sleep(1000);
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
