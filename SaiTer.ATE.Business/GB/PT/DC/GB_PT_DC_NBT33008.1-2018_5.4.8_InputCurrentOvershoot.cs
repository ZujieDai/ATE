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
    /// 国标产测直流：输入电流过冲试验
    /// </summary>
    public class GB_PT_DC_InputCurrentOvershoot : BusinessBase
    {
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        double errorPecent = 10;//误差百分比
        public GB_PT_DC_InputCurrentOvershoot(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            errorPecent = Convert.ToDouble(strParams[0].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 3)
            {
                BMSDemandVolt = Convert.ToDouble(strParams[1].Split('=')[1]);
                ResiLoadCurrent = Convert.ToDouble(strParams[2].Split('=')[1]);
            }
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

                    if (ControlEquipMent.FeedbackLoad != null)
                    {
                        ControlEquipMent.FeedbackLoad.FeedbackLoad_Parallel(testWorkParam.lstIDs);
                    }
                    else
                    {
                        //string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                        //if (Customer != null && Customer.Contains("GJ"))
                        //{
                        //    bool[] State = new bool[16];
                        //    State[6] = true;
                        //    State[8] = true;
                        //    ControlEquipMent.ControlBoard?.ControlResistanceSetRelay(State.ToList());
                        //}
                    }
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_Parallel(testWorkParam.lstIDs);
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
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "500", "0");//通道3设置 惠州TB100：1

                    //临时加的
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && Customer.Contains("XJ"))
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "500", "0");//通道3设置 惠州TB100：1
                    }
                    else
                    {
                        ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "AC", "20", Channel3, "Input_AC_I", "1", "A", false, "100", "0");//通道3设置 惠州TB100：1
                    }

                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "2M");
                    //设置时基ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0.4");
                    //添加测量值
                    //ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//
                    //Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RMS", 3);//
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MAX", 3);//
                    Thread.Sleep(waitTime);




                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //设置测试条件
                    SetConditionValues();

                    Dictionary<int, string> dicInCurrent = new Dictionary<int, string>();
                    Dictionary<int, string> dicInACImax = new Dictionary<int, string>();
                    Dictionary<int, string> dicInACImax2 = new Dictionary<int, string>();
                    Dictionary<int, string> dicInACTOP = new Dictionary<int, string>();


                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 1, BMSDemandVolt, 1);
                    //Thread.Sleep(1000);
                    //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, 0, true, BMSDemandVolt);

                    SendNoticeToUIAndTxtFile("设备开启负载中，请稍候...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "90", "Single");//上升边沿触发
                    Thread.Sleep(300);
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent + 10, BMSDemandVolt - 5, ResiLoadCurrent);
                    Thread.Sleep(300);
                    SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(300);
                    Dictionary<int, bool> dicTemp = new Dictionary<int, bool>();
                    // 受负载的影响，第一次触发是负载接触器触发的电流不是充电桩的
                    int count = 30;
                    while (count-- > 0)
                    {
                        dicTemp = ControlEquipMent.Oscilloscope.ReadTriggerState(testWorkParam.lstIDs);

                        if (dicTemp[testWorkParam.lstIDs[0]])
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "90", "Single");//上升边沿触发

                            Thread.Sleep(100);
                        }
                        else
                        { break; }

                    }

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, ResiLoadCurrent, false, BMSDemandVolt);
                    Thread.Sleep(1000 * 15);

                    dicInACImax = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 3); //采集的是输入交流电流，最大值
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Auto");
                    Thread.Sleep(1000 * 3);

                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString("F2"));
                        d2.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(d1, BMSDemandVolt + "V," + ResiLoadCurrent + "A", "直流输出电压(V)", "-", "-");
                    ProcessDataTmp(d2, BMSDemandVolt + "V," + ResiLoadCurrent + "A", "直流输出电流(A)", "-", "-");


                    dicInCurrent = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RMS", 3); //采集的是输入交流电流，最大值
                    ProcessDataTmp(dicInCurrent, BMSDemandVolt + "V," + ResiLoadCurrent + "A", "额定输入电流均方根", "-", "-");
                    //dicInACTOP = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3); //采集的是额定输入电流峰值
                    //ProcessDataTmp(dicInACTOP, BMSDemandVolt + "V," + ResiLoadCurrent + "A", "额定输入电流顶端值", "-", "-");


                    ProcessDataTmp(dicInACImax, BMSDemandVolt + "V," + ResiLoadCurrent + "A", "启动输入电流最大值", "-", "-", dImgs);

                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    dicInACImax2.Clear();
                    //dicInACImax2 = ProcessOscillcopeData();
                    dicInACImax2 = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 3); //采集的是输入交流电流，最大值
                    //dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dicInACImax2, BMSDemandVolt + "V," + ResiLoadCurrent + "A", "输入电流对比值", "-", "-", dImgs);


                    double InCurrentIppSet = Convert.ToDouble(dicInCurrent[testWorkParam.lstIDs[0]]) * 1.414213562373095;

                    Double ErrorValue = 0;


                    Double ErrorValue1 = System.Math.Abs(Convert.ToDouble(dicInACImax[testWorkParam.lstIDs[0]]) * 100 / InCurrentIppSet); //误差   最大值==峰值
                    Double ErrorValue2 = System.Math.Abs(Convert.ToDouble(dicInACImax[testWorkParam.lstIDs[0]]) * 100 / Convert.ToDouble(dicInACImax2[testWorkParam.lstIDs[0]]));
                    if (ErrorValue2 == 0)
                    {
                        ErrorValue = ErrorValue1;
                    }
                    else
                    {
                        ErrorValue = ErrorValue1 > ErrorValue2 ? ErrorValue2 : ErrorValue1;
                    }


                    Dictionary<int, string> dicResultData = new Dictionary<int, string>();
                    dicResultData.Add(1, ErrorValue.ToString("F2"));

                    ProcessDataTmp(dicResultData, "电流过冲", "启动电流MAX/稳定电流MAX(%)", "-", (100 + errorPecent).ToString("F2"));


                    SetLoadDCOFF(testWorkParam.lstIDs);
                    Thread.Sleep(200);
                    SendNoticeToUIAndTxtFile("负载取消并机");
                    ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                    ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);
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

        public Dictionary<int, string> ProcessOscillcopeData()
        {

            Dictionary<int, string> dicResult = new Dictionary<int, string>();
            try
            {
                Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(testWorkParam.lstIDs, 3);

                if (datas == null)
                {
                    return null;
                }
                foreach (var item in datas)
                {
                    if (item.Value == null)
                    {
                        return null;
                    }

                    int count = Convert.ToInt32(item.Value.Length / 10 * 9);//取最后十分之一的数据
                    double maxValue = 0;
                    for (int i = item.Value.Length - 1; i > count; i--)
                    {
                        if (Math.Abs(item.Value[i]) > maxValue)
                        {
                            maxValue = Math.Abs(item.Value[i]);
                        }
                    }
                    dicResult.Add(item.Key, maxValue.ToString("F2"));
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }
            return dicResult;
        }
    }
}
