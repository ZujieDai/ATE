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
    /// 二极管短路测试
    /// </summary>
    public class DiodeShortCircuit_AC : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        private string maxValue = "100";
        string trigMinVol = "-10";      //触发矮脉冲下限
        string trigMaxVol = "4";        //触发矮脉冲上限

        public DiodeShortCircuit_AC(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
            string[] TrialParams = TrialItem.ResultParams.Split('|')[0].Split('=');

            if (TrialParams.Length == 2)
            {
                maxValue = TrialParams[1];
            }

            string[] ItemParams = TrialItem.ItemParams.Split('|');
            if (ItemParams.Length >= 2)
            {
                trigMinVol = ItemParams[0].Split('=')[1];
                trigMaxVol = ItemParams[1].Split('=')[1];
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

                var Ks = GetKStatus16_Charging();
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);

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

                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                ////检测刷卡
                //WaitSwipingCard(testWorkParam.lstIDs, 0);


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
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "2.5");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    Thread.Sleep(waitTime);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    //Thread.Sleep(waitTime);
                    //添加测量值
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                    Thread.Sleep(waitTime);

                    //设置触发（二极管短路不需要触发，只验证负电压就行了
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", trigMaxVol, 3, trigMinVol, "Auto");
                    Thread.Sleep(waitTime);
                    //设置时基400ms
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "1");//低值
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);


                    //闭合开关S2，启动充电
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    //检测是否充电
                    WaitSwipingCard(testWorkParam.lstIDs, 0);

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

                    Thread.Sleep(4000);

                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("模拟二极管短路");
                    var Ks = GetKStatus16_Charging();
                    Ks[15] = true;
                    ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                    Thread.Sleep(3000);
                    SendNoticeToUIAndTxtFile("正在分析示波器数据");
                    //读取分析数据（CP负电压-12变为-6，并且停止充电）
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                    Thread.Sleep(1000);
                    //CPPWMNegativeUpTime(testWorkParam.lstIDs, 3, -8, 1);//CP动作时间
                    //ACDownTime(testWorkParam.lstIDs, 1, 55, 2);//AC回路动作时间

                    ////读取卡点时间
                    //ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                    //Data_Tmp = GetOSCTime(OscTime_Tmp);
                    //Dictionary<int, string> dd = new Dictionary<int, string>();
                    //foreach (var itmp in Data_Tmp)
                    //{
                    //    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    //}
                    //dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    //ProcessDataTmp(dd, "二极管短路", "S1开关切换时间(ms)", "0", maxValue, dImgs);

                    //读取C状态CP电压值
                    Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//高值
                    ProcessDataTmp(Data_Tmp, "二极管短路", "CP负电压(V)", "-6.8", "-5.2");

                    //断线后的电压
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "二极管短路", "短路后桩输出电压(V)", "0", "20");

                }
            }
        }



        public override void ProcessData()
        {

        }


    }
}
