using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class NormalEndCharge_AC_RT : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        /// <summary>
        /// K1 K2断开时间
        /// </summary>
        double BreakTime = 0;//
        /// <summary>
        /// 负载需求电流
        /// </summary>
        double DemandCurrent = 0;

        double DemandVoltage = 220;
        /// <summary>
        /// 示波器波形回读数据
        /// </summary>
        Dictionary<int, double[]> dicOscilloscopeCursorData = new Dictionary<int, double[]>();
        public NormalEndCharge_AC_RT(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BreakTime = double.Parse(strParams[0].Split('=')[1]);
            //DemandCurrent = double.Parse(strParams[1].Split('=')[1]);
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
                //设置测试条件
                SetConditionValues();


                if (testWorkParam.lstIDs.Count > 0)
                {
                    SetCPReresh();

                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    //3通道 打开  DC耦合   带宽  探头比   标签      阻抗   电压  反向通道  纵坐标档位  纵坐标位置
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);

                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "1.25M");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    Thread.Sleep(waitTime);
                    //设置时基
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1");//滚动  时基mS   延时
                    Thread.Sleep(waitTime);
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    Thread.Sleep(waitTime);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    foreach (var item in ControlEquipMent.Oscilloscope.DitEquipMentBase)
                    {
                        if (item.Value.GetType() == typeof(DLMOscilloscope))
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8.5", "Single");//上升边沿触发，自动
                        }
                        else if (item.Value.GetType() == typeof(emtTekOscilloscope))
                        {
                            if (Customer != null && Customer.ToString().ToUpper().Contains("HYQCP"))
                            {
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                                //设置时基
                                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "1.2");//滚动  时基mS   延时
                            }
                            else
                            {
                                //超时触发，泰克示波器在设备层做了触发模式转换
                                //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "FALL", "DC", "EDGE", "50", 1, "100", "Auto");
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                                Thread.Sleep(waitTime);
                                //设置时基
                                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "1.2");//滚动  时基mS   延时
                            }
                        }
                        else if (item.Value.GetType() == typeof(SDSOscilloscope))
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                        }
                    }


                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    //Thread.Sleep(2000);

                    //设置触发
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");  //设置单次触发
                    Thread.Sleep(waitTime);
                    Thread.Sleep(4500);

                    //模拟S2断开
                    //List<bool> Ks = GetKStatus16_Charging();
                    //Ks[0] = false;
                    //ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(4500);

                    //读取分析数据
                    //泰克示波器需要等待5秒以上才可以绘制出波形图
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null
                        || ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope_MDO34")) != null)
                        Thread.Sleep(4000);
                    else
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1, 1250000.0);//CP动作时间
                    ACDownTime(testWorkParam.lstIDs, 1, 55, 2, 1250000.0, 5);//AC回路动作时间

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, itmp.Value.ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "被动终止充电", "K1K2断开时间(ms)", "0", BreakTime.ToString(), dImgs);


                    #region 主动终止充电--3s内断开S2
                    SetCPReresh();

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    Thread.Sleep(waitTime);
                    foreach (var item in ControlEquipMent.Oscilloscope.DitEquipMentBase)
                    {
                        if (item.Value.GetType() == typeof(DLMOscilloscope))
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8.5", "Single");//上升边沿触发，自动
                        }
                        else if (item.Value.GetType() == typeof(emtTekOscilloscope))
                        {
                            if (Customer != null && Customer.ToString().ToUpper().Contains("HYQCP"))
                            {
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                                Thread.Sleep(waitTime);
                                //设置时基
                                //ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "-1.2");//滚动  时基mS   延时
                                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "-1.8");//滚动  时基mS   延时
                            }
                            else
                            {
                                //超时触发，泰克示波器在设备层做了触发模式转换
                                //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "FALL", "DC", "EDGE", "50", 1, "100", "Auto");
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动

                                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "1");//滚动  时基mS   延时
                                Thread.Sleep(waitTime);
                            }
                        }
                        else if (item.Value.GetType() == typeof(SDSOscilloscope))
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                            Thread.Sleep(waitTime);
                            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "-1.8");
                        }
                    }


                    Thread.Sleep(10000);

                    //设置触发
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");  //设置单次触发
                    Thread.Sleep(waitTime);
                    //Thread.Sleep(4500);

                    //监测是否手动停止充电，手动停止充电后3s内需要断开S2
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                int time = 300;
                                while (time-- > 0)
                                {
                                    var CPFrequency = AllEquipStateData.DicBMS_AC_StateData[item].CPFrequency;
                                    if (CPFrequency < 10)
                                    {
                                        time -= 3;
                                        Thread.Sleep(300);
                                        CPFrequency = AllEquipStateData.DicBMS_AC_StateData[item].CPFrequency;
                                        if (CPFrequency < 10)
                                        {
                                            SendNoticeToUIAndTxtFile($"检测到PWM停止，枪{item} 1.2秒后断开S2");
                                            Thread.Sleep(1200);
                                            var Ks = GetKStatus16_Charging();
                                            Ks[0] = false;
                                            ControlEquipMent.BMS.BMS_SetKState(new List<int>() { item }, Ks);
                                            SendNoticeToUIAndTxtFile($"枪{item} S2已断开");
                                            return;
                                        }
                                    }
                                    Thread.Sleep(100);
                                }
                            }
                            catch (Exception ex)
                            {
                                SendException(ex);
                            }
                        });
                    }
                    CountDownTimeInfo("请手动停充后点击确认，或等待倒计时结束。", 60, 0);
                    Thread.Sleep(1000);

                    Thread.Sleep(5000);

                    //读取分析数据
                    //泰克示波器需要等待5秒以上才可以绘制出波形图
                    if (ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope")) != null
                        || ControlEquipMent.Oscilloscope.DitEquipMentBase.Values.FirstOrDefault(e => e.EquipMentClassName.Equals("emtTekOscilloscope_MDO34")) != null)
                        Thread.Sleep(4000);
                    else
                        ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1, 1250000.0);//CP动作时间
                    //CPPWMDownTime(testWorkParam.lstIDs, 3, -11.2, 2, 1);
                    ACDownTime(testWorkParam.lstIDs, 1, 55, 2, 1250000.0, 3);//AC回路动作时间

                    Thread.Sleep(2000);

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, itmp.Value.ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "主动充电（3s内断开开关S）", "K1K2断开时间(ms)", "0", BreakTime.ToString(), dImgs);
                    #endregion

                    #region 主动终止充电--3s后断开S2
                    SetCPReresh();

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    foreach (var item in ControlEquipMent.Oscilloscope.DitEquipMentBase)
                    {
                        if (item.Value.GetType() == typeof(DLMOscilloscope))
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8.5", "Single");//上升边沿触发，自动
                        }
                        else if (item.Value.GetType() == typeof(emtTekOscilloscope))
                        {
                            if (Customer != null && Customer.ToString().ToUpper().Contains("HYQCP"))
                            {
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                                //设置时基
                                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "-1.8");//滚动  时基mS   延时
                            }
                            else
                            {
                                //超时触发，泰克示波器在设备层做了触发模式转换
                                //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 1, "FALL", "DC", "EDGE", "50", 1, "100", "Auto");
                                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                                Thread.Sleep(waitTime);

                                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "-2");//滚动  时基mS   延时
                                Thread.Sleep(waitTime);
                            }
                        }
                        else if (item.Value.GetType() == typeof(SDSOscilloscope))
                        {
                            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "8", "Single");//上升边沿触发，自动
                            Thread.Sleep(waitTime);
                            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "-1.8");
                        }
                    }


                    Thread.Sleep(10000);

                    //设置触发
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");  //设置单次触发
                    Thread.Sleep(waitTime);
                    //Thread.Sleep(4500);

                    //监测是否手动停止充电，手动停止充电后3后需要断开S2
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                int time = 300;
                                while (time-- > 0)
                                {
                                    var CPFrequency = AllEquipStateData.DicBMS_AC_StateData[item].CPFrequency;
                                    if (CPFrequency < 10)
                                    {
                                        time -= 3;
                                        Thread.Sleep(300);
                                        CPFrequency = AllEquipStateData.DicBMS_AC_StateData[item].CPFrequency;
                                        if (CPFrequency < 10)
                                        {
                                            SendNoticeToUIAndTxtFile($"检测到PWM停止，枪{item} 3秒后断开S2");
                                            Thread.Sleep(3000);
                                            var Ks = GetKStatus16_Charging();
                                            Ks[0] = false;
                                            ControlEquipMent.BMS.BMS_SetKState(new List<int>() { item }, Ks);
                                            SendNoticeToUIAndTxtFile($"枪{item} S2已断开");
                                            return;
                                        }
                                    }
                                    Thread.Sleep(100);
                                }
                            }
                            catch (Exception ex)
                            {
                                SendException(ex);
                            }
                        });
                    }
                    CountDownTimeInfo("请手动停充后点击确认，或等待倒计时结束。", 60, 0);
                    //Thread.Sleep(4000);
                    Thread.Sleep(9000);
                    if(ControlEquipMent.Oscilloscope.DitEquipMentBase.FirstOrDefault().Value.GetType() == typeof(emtTekOscilloscope))
                        Thread.Sleep(5 * 1000);

                    //读取分析数据
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    //CPPWMUpTime(testWorkParam.lstIDs, 3, 8, 1);//CP动作时间
                    CPPWMDownTime(testWorkParam.lstIDs, 3, -6, 1, 1, 1250000.0, 1);
                    ACDownTime(testWorkParam.lstIDs, 1, 100, 2, 1250000.0, 4);//AC回路动作时间

                    Thread.Sleep(2000);

                    //读取卡点时间
                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, itmp.Value.ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "主动充电（3s后断开开关S）", "K1K2断开时间(ms)", "3000", (3000 + BreakTime).ToString(), dImgs);
                    #endregion
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


                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;

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
