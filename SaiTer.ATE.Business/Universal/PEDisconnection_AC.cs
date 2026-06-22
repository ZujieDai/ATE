using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 交流：PE断线（程控板控制）
    /// </summary>
    public class PEDisconnection_AC : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        string disConnectionTime = "100";//断线上限时间
        int PESamplingIndex = 8;   //PE信号采集对应的程控板开关
        bool PEStatus = true;//PE状态，true为闭合未断线，false为断开未断线

        public PEDisconnection_AC(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            disConnectionTime = strParams[0].Split('=')[1];

            //PE信号控制(索引号)=8
            string[] itemParams = TrialItem.ItemParams.Split('|');
            if (itemParams.Length > 0 && itemParams[0].Split('=').Length > 1)
            {
                PESamplingIndex = Convert.ToInt32(itemParams[0].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {

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
                var lstRelay = ControlEquipMent.ControlBoard.ControlBoardReadState(lstIDs);
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                lstRelay[PESamplingIndex] = false;
                if (Customer != null && Customer.Contains("YTZL_ACDC"))
                {
                    lstRelay[10] = false;
                }
                lstRelay[14] = PEStatus;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstIDs, lstRelay);
                SetCPReresh();

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }


        /// <summary>
        /// 测试流程
        /// </summary>
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //升源，启动BMS
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }

                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                ////检测刷卡
                //WaitSwipingCard(testWorkParam.lstIDs, 0);

                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();

                if (testWorkParam.lstIDs.Count > 0)
                {
                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, true, "DC", "20", Channel4, "PE_Voltage", "50", "V", false, "10", "1");
                    Thread.Sleep(waitTime);

                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 4);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 4);//低值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    Thread.Sleep(waitTime);
                    //设置触发（泰克DPO 2020B需要先设置触发再设置延迟，不然不会生效）
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 4, "6", "Single");//下降边沿触发，自动


                    //通道4切换为PE信号采集
                    var lstRelay = ControlEquipMent.ControlBoard.ControlBoardReadState(testWorkParam.lstIDs);
                    lstRelay[PESamplingIndex] = true;
                    if (Customer != null && Customer.Equals("HYQCP"))
                    {
                        lstRelay[13] = true;
                    }
                    if (Customer != null && Customer.Contains("YTZL_ACDC"))
                    {
                        lstRelay[10] = true;
                    }
                    //lstRelay[14] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(testWorkParam.lstIDs, lstRelay);
                    PEStatus = lstRelay[14];
                    //启动示波器
                    //启动输出的时候可能会有下降到6V的杂波提前触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 4, "4", "Atuo");
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    //防止信号误触发
                    Thread.Sleep(3500);

                    //闭合开关S2，启动充电
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                        {
                            SendNoticeToUIAndTxtFile("未检测到" + testWorkParam.lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                            testWorkParam.lstIDs.Remove(testWorkParam.lstIDs[i]);
                        }
                    }

                    //无充电桩在测试，直接退出 
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    //让示波器跑一会儿
                    Thread.Sleep(3000);

                    //设置测试条件
                    SetConditionValues();

                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    Thread.Sleep(3000);
                    //设置时基
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0.2");//低值
                    if (Convert.ToDouble(disConnectionTime) >= 4000)
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "3");//低值
                    else if(Convert.ToDouble(disConnectionTime) <= 100)
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "200", "0.8");//低值
                    else
                        ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "1.5");//低值
                    Thread.Sleep(3000);

                    //模拟PE断线
                    SendNoticeToUIAndTxtFile("发送断线");
                    //lstRelay[14] = false;
                    lstRelay[14] = !lstRelay[14];
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(testWorkParam.lstIDs, lstRelay);

                    //Thread.Sleep(700);
                    Thread.Sleep(3700);
                    //泰克示波器需要等待5秒以上才可以绘制出波形图
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null)
                        Thread.Sleep(13000);
                    else if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope_MDO34")) != null)
                        Thread.Sleep(5000);
                    else
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    //读取分析数据
                    Thread.Sleep(1000);
                    //CPPWMDownTime(testWorkParam.lstIDs, 4, 6, 1);//PE动作时间
                    CPPWMDownTime(testWorkParam.lstIDs, 4, 6, 1, 2);
                    //Dictionary<int, double> records = new Dictionary<int, double>();
                    //records.Add(testWorkParam.lstIDs.First(), 600.0 / (400.0 * 10.0));
                    //ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, 4, records, true);   //触发位置
                    if (Convert.ToDouble(disConnectionTime) >= 4000)
                        ACDownTime(testWorkParam.lstIDs, 1, 55, 2, 200000, 2);//AC回路动作时间
                    else
                        ACDownTime(testWorkParam.lstIDs, 1, 55, 2);//AC回路动作时间
                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "PE断线", "电源切断时间(ms)", "0", disConnectionTime, dImgs);

                    //断线后的电压
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "PE断线", "断线后输出电压(V)", "0", "30");

                    CountDownTimeInfo("请确认电子锁是否正确解锁。\r\n注：勾选上为正确解锁", 20, 2);
                    ProcessDataConnect("PE断线", "是否正确解锁");

                }
            }
        }

        public override void ProcessData()
        {

        }
    }
}
