using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 动力电源输入失电_交流
    /// </summary>
    public class PowerInputLost : BusinessBase
    {

        double FreqMax = 1020;
        double FreqMin = 980;
        double maxTime = 1000;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        int TrialFlowNum = 1;
        private int sleepTime = 2000;
        private int timeBase = 500;     //示波器时基
        private double timeOffset = 1;    //示波器偏移

        public PowerInputLost(int trialType) { TrialType = trialType; }

        public override void InitEquiMent()
        {
            //  AllEquipStateData.DicBMS_AC_StateData[]
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            //FreqMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            //FreqMin = Convert.ToDouble(strParams[0].Split('=')[1]);
            maxTime = double.Parse(strParams[0].Split('=')[1]);
            string[] ItemParams = TrialItem.ItemParams.Split('|')[0].Split('=');
            if (ItemParams.Length == 2)
            {
                sleepTime = Convert.ToInt32(double.Parse(ItemParams[1]));
            }
            string[] strItems = TrialItem.ItemParams.Split('|');
            if (strItems.Length > 1)
            {
                timeBase = Convert.ToInt32(double.Parse(strItems[1].Split('=')[1]));
                timeOffset = double.Parse(strItems[2].Split('=')[1]);
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                SetCPReresh();

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
                    SendNoticeToUIAndTxtFile("正在检测插枪状态");
                    if (!CheckChargerIn(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    SendNoticeToUIAndTxtFile("正在检测充电桩上电状态");

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    /*
                     *  4.示波器打开CH1通道，100V/格，400ms/格；
                        5.触发CH1，上升沿，0V电平，自动模式
                        6.按照额定电流投载
                        7.发送关源，2秒后，停止示波器
                        8.读取CH1数据，X1从右到左卡在第一个大于±200V的绝对值（客户输入的电压值-10V），X2从X1的位置开始从左至右卡在第一个小于±42.4V的绝对值
                        9.判断ΔX＜1s，合格，否则fail；
                     */

                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    SendNoticeToUIAndTxtFile("关闭示波器2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "AC-out-A", "50", "V", false, "50", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, true, "DC", "20", Channel4, "AC-in-V", "50", "V", false, "250", "2.0");
                    SendNoticeToUIAndTxtFile("开启示波器1号通道，并设置相应参数");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "AC-out-V", "50", "V", false, "250", "-2.5");

                    //设置触发（泰克DPO 2020B需要先设置触发再设置延迟，不然不会生效）
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "100", 1, "0mV", "Auto");
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", "160.0", 4, "-160.0", "Single");//上升边沿触发，自动

                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, timeBase.ToString(), timeOffset.ToString());
                    Thread.Sleep(waitTime);

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


                    SendNoticeToUIAndTxtFile("正在开启负载,等待电流稳定");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                    //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3000);

                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    SendNoticeToUIAndTxtFile("关闭交流源,等待" + sleepTime +"ms后停止示波器滚动");
                    ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(sleepTime);
                    SendNoticeToUIAndTxtFile("正在读取示波器数据");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    //分析波形数据
                    //ACDownTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage * 0.95, 1);//CP动作时间
                    ACDownTime(testWorkParam.lstIDs, 4, 60, 1);
                    ACDownTime(testWorkParam.lstIDs, 1, 60, 2);//AC回路动作时间


                    //Dictionary<int, string>  DataU_Tmp = new Dictionary<int, string>();
                    //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    //{
                    //    double voltage = 0;
                    //    voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                    //    DataU_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString());
                    //}
                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        double voltage = 0;
                        int iindex = 0;
                        iindex = itmp.Key - 1;
                        if (iindex < 0) iindex = 0;
                        voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[iindex]].PhaseA_Voltage;
                        if(voltage > 80)//默认未停止输出，时间很大
                        {
                            dd.Add(itmp.Key, (999999).ToString());
                        }
                        else//正常停止
                        {
                            dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                        }
                        //dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "动力电源输入失电", "降至42.4以下时间(ms)", "0", maxTime.ToString(), dImgs);


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
