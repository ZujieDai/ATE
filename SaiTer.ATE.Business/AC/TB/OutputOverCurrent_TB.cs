using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出过流测试
    /// </summary>
    public class OutputOverCurrent_TB : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电

        double OutputCurrent = 40;//动作电流
        double DemandCurrent = 30;//不动作电流
        public OutputOverCurrent_TB(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputCurrent = Convert.ToDouble(strParams[0].Split('=')[1]) + 0.5;
            DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            IsCardCharg = double.Parse(strParams[2].Split('=')[1]) == 1;
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
                // TB欧标桩需要刷卡才能结束充电，并且等待CP波纹和充电电压为0
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                {
                    CountDownTimeInfo("请刷卡终止充电，并等待充电桩结算后点击确认", 999, 0);
                    int i = 500;
                    while (i-- > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                        {
                            //双重判断
                            Thread.Sleep(100);
                            if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                                break;
                        }
                        Thread.Sleep(100);
                    }
                }
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                //SetCPReresh();

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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + DemandCurrent + "|" + OutputCurrent + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count < 0)
                {

                    return;
                }
                SendNoticeToUIAndTxtFile("检测充电桩充电状态。");


                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                SetConditionValues();


                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, DemandCurrent);


                SendNoticeToUIAndTxtFile("已发送负载不动作电流值：" + DemandCurrent + "A，等待负载稳定。");
                Thread.Sleep(1000 * 12);//防止正常电流出现过流保护，需要超过十秒的时间
                //采集数据
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString("F2"));
                }
                ProcessDataTmp(Data_Tmp, "不动作电流", "输出电压(V)", "80", "260");

                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent, false);

                SendNoticeToUIAndTxtFile("已发送负载过流值：" + OutputCurrent + "A，等待负载稳定。");

                //电流可能到不了设定值
                int timeout = 20;
                while (timeout-- > 0)
                {
                    double current = AllEquipStateData.DicBMS_AC_StateData[LstChargerInfo[0].ChargerId].PhaseA_Current;
                    if (Math.Abs(current - OutputCurrent) < OutputCurrent * 0.05)
                        break;
                    Thread.Sleep(300);
                }
                if (timeout > 0)
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent * 1.05, false);


                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    timeout = 35;
                    double volt = 0;
                    int count = 0;
                    while (timeout-- > 0)
                    {
                        volt = AllEquipStateData.DicBMS_AC_StateData[LstChargerInfo[0].ChargerId].PhaseA_Voltage;

                        if (volt < 20)
                        {
                            count++;
                            if (count >= 3)
                            {
                                break;
                            }
                        }
                        Thread.Sleep(1000);
                    }

                    Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString("F2"));
                }
                ProcessDataTmp(Data_Tmp, "过流保护", "输出电压(V)", "0", "30");

                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                int timeOut = 10;
                double acvolt = 0;
                int account = 0;
                while (timeOut-- > 0)
                {
                    acvolt = AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt;

                    if (acvolt < 20)
                    {
                        account++;
                        if (account >= 3)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(300);
                }
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
            }           
        }


        public override void ProcessData()
        {

        }
    }
}
