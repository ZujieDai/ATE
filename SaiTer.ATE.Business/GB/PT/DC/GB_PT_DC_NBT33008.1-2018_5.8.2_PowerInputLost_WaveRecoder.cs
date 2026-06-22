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
    /// 国标产测直流：动力电源失电试验(新录波板)，目前录波板未接入输入信号，此类无效
    /// </summary>
    public class GB_PT_DC_PowerInputLost_WaveRecoder : BusinessBase
    {
        double maxTime = 1000;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        int TrialFlowNum = 1;
        private int sleepTime = 2000;
        private int timeBase = 500;     //示波器时基
        private double timeOffset = 1;    //示波器偏移
        public GB_PT_DC_PowerInputLost_WaveRecoder(int trialType) { TrialType = trialType; }

        public override void InitEquiMent()
        {
            //  AllEquipStateData.DicBMS_AC_StateData[]
        }

        public override void InitializeParams()
        {
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            maxTime = double.Parse(strParams[0].Split('=')[1]);


            string[] strItems = TrialItem.ItemParams.Split('|');
            if (strItems.Length >= 2)
            {
                timeBase = Convert.ToInt32(double.Parse(strItems[0].Split('=')[1]));
                timeOffset = double.Parse(strItems[1].Split('=')[1]);
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

                                //LstTrialData[i].ExtentData = "null|null|" + FreqMin + "|" + FreqMax;

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                if (testWorkParam.lstIDs.Count > 0)
                {
                    int waitTime = 50;
                    SendNoticeToUIAndTxtFile("正在检测充电桩上电状态");
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //设置测试条件
                    SetConditionValues();

                    if (ControlEquipMent.ResistanceLoad != null)
                    {
                        SendNoticeToUIAndTxtFile("正在开启负载,等待电流稳定");
                        SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, 20, LstChargerInfo[0].NominalVoltage, 20);
                        Thread.Sleep(500);
                        SetLoadDCON(testWorkParam.lstIDs);
                        Thread.Sleep(1000 * 15);
                    }
                    /*
                     *  4.示波器打开CH1通道，100V/格，400ms/格；
                        5.触发CH1，上升沿，0V电平，自动模式
                        6.按照额定电流投载
                        7.发送关源，2秒后，停止示波器
                        8.读取CH1数据，X1从右到左卡在第一个大于±200V的绝对值（客户输入的电压值-10V），X2从X1的位置开始从左至右卡在第一个小于±42.4V的绝对值
                        9.判断ΔX＜1s，合格，否则fail；
                     */

                    SendNoticeToUIAndTxtFile("关闭示波器3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "DC-out-A", "50", "A", false, "10", "-2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, true, "AC", "20", Channel4, "Input_AC_V", "50", "V", false, "300", "2");
                    SendNoticeToUIAndTxtFile("开启示波器1、4号通道，并设置相应参数");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "DC-out-V", "50", "V", false, "250", "-3");

                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基2ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, timeBase.ToString(), timeOffset.ToString());
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MIN", 1);
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "100", 1, "60V", "Single");
                    Thread.Sleep(timeBase * 10);



                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    SendNoticeToUIAndTxtFile("关闭交流源,等待");
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(sleepTime);
                    SendNoticeToUIAndTxtFile("正在读取示波器数据");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //分析波形数据
                    ACDownTime(testWorkParam.lstIDs, 4, 50, 1);
                    ACDownTime(testWorkParam.lstIDs, 1, 60, 2);

                    //读取卡点时间 注意：如果直流输出电压下降比输入电压的低电压余波早，△t大于0实际应按0来处理
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "动力电源输入失电", "降至60V以下时间(ms)", "0", maxTime.ToString(), dImgs);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    //延时5秒判断不会继续充电
                    Thread.Sleep(5000);
                    d1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt.ToString("f2"));
                    }
                    ProcessDataTmp(d1, "交流源恢复供电不应继续充电", "充电电压(V)", "0", "30");
                }
            }
        }
        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {


                }
            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
