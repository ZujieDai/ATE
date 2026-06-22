using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 动力电源输入失电_交流  (改用1、4通道测试。 临时类，暂时用于安徽NT项目，测试稳定后再合并到主版本)
    /// </summary>
    public class PowerInputLost_tmp : BusinessBase
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

        public PowerInputLost_tmp(int trialType) { TrialType = trialType; }

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

                    SendNoticeToUIAndTxtFile("关闭示波器2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "AC-out-A", "50", "V", false, "50", "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, true, "DC", "20", Channel4, "AC-in-V", "50", "V", false, "250", "2.5");
                    SendNoticeToUIAndTxtFile("开启示波器1号通道，并设置相应参数");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "AC-out-V", "50", "V", false, "250", "-2.5");

                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "RISE", "DC", "EDGE", "50", 1, "0", "Single");//上升超时触发，单次
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, timeBase.ToString(), timeOffset.ToString());
                    Thread.Sleep(waitTime);

                    //设置触发

                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "100", 1, "-100", "Single");//上升边沿触发，自动
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "Alternating", "DC", "EDGE", "50", 1, "100", "Single");//上升边沿触发，自动
                    SendNoticeToUIAndTxtFile("正在开启负载,等待电流稳定");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 10);
                    Thread.Sleep(2000);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(5000);
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.Contains("TRW"))
                    {
                        Thread.Sleep(1000 * 20);
                    }
                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                    //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);


                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("关闭交流源,等待" + sleepTime + "ms后停止示波器滚动");
                    //可能没有交流源，通过继电器电网输入
                    ControlEquipMent.ACSource?.ACSource_OFF(new List<int> { 1 });
                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("正在读取示波器数据");
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    //分析波形数据
                    ACDownTime(testWorkParam.lstIDs, 4, 42.4, 1);//
                    ACDownTime(testWorkParam.lstIDs, 1, 42.4, 2);//AC回路动作时间


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
                    foreach (int chargerId in testWorkParam.lstIDs)
                    {
                        double voltage = 0;
                        voltage = AllEquipStateData.DicBMS_AC_StateData[chargerId].PhaseA_Voltage;
                        if (voltage > 80)//默认未停止输出，时间很大
                        {
                            dd.Add(chargerId, (999999).ToString());
                        }
                        else//正常停止
                        {
                            dd.Add(chargerId, (Convert.ToDouble(Data_Tmp[chargerId])).ToString());
                        }
                        //dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "动力电源输入失电", "降至42.4以下时间(ms)", "0", maxTime.ToString(), dImgs);

                    ControlEquipMent.ACSource?.ACSource_ON(new List<int> { 1 });
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
